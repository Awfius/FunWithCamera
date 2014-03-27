using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunWithCamera.Core.ViewModels
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		#region Private Members

		private static Action _emptyAction = () => { };

		#endregion


		#region Public Methods

		public async Task Activate()
		{
			await OnActivate();
		}

		public async Task Deactivate()
		{
			await OnDeactivate();
		} 

		#endregion


		#region Events

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion


		#region Protected Methods

		protected void NotifyPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion


		#region Event Handling

		protected virtual Task OnActivate()
		{
			return TaskEx.Run(_emptyAction);
		}

		protected virtual Task OnDeactivate()
		{
			return TaskEx.Run(_emptyAction);
		}

		#endregion
	}

	public class FilterViewModel : ViewModelBase
	{
		#region Public Properties

		public Filter Filter { get; private set; }

		public string Name { get; private set; }

		#endregion


		#region Constructors

		public FilterViewModel(Filter filter)
		{
			Filter = filter;
			Name = filter.ToString();
		}

		#endregion
	}

	public class MainViewModel : ViewModelBase
	{
		#region Private Members

		private readonly ObservableCollection<FilterViewModel> _filters = new ObservableCollection<FilterViewModel>();

		private FilterViewModel _selectedFilter;

		private int _captureCountdown = 3;

		private int _captureCountdownOpacity = 0;

		#endregion

		#region Public Properties

		public ReadOnlyObservableCollection<FilterViewModel> Filters { get; private set; }

		public FilterViewModel SelectedFilter
		{
			get { return _selectedFilter; }
			set 
			{ 
				if (_selectedFilter == value)
				{
					return;
				}

				_selectedFilter = value;
				NotifyPropertyChanged("SelectedFilter");

				Debug.WriteLine("SelectedFilter: {0}", value.Name);
			}
		}

		public int CaptureCountdown
		{
			get { return _captureCountdown; }
			set
			{
				if (_captureCountdown == value)
				{
					return;
				}

				_captureCountdown = value;
				NotifyPropertyChanged("CaptureCountdown");
			}
		}

		public int CaptureCountdownOpacity
		{
			get { return _captureCountdownOpacity; }
			set
			{
				if (_captureCountdownOpacity == value)
				{
					return;
				}

				_captureCountdownOpacity = value;
				NotifyPropertyChanged("CaptureCountdownOpacity");
			}
		}

		#endregion

		#region Constructors

		public MainViewModel()
		{
			Filters = new ReadOnlyObservableCollection<FilterViewModel>(_filters);
		}

		#endregion

		#region Event Handling

		protected override async Task OnActivate()
		{
			var filters = await TaskEx.Run(() =>
			{
				var list = new List<FilterViewModel>();
				foreach (var filter in typeof(Filter).GetFields())
				{
					if (filter.IsLiteral)
					{
						list.Add(new FilterViewModel((Filter)filter.GetValue(typeof(Filter))));
					}
				}
				return list;
			});

			foreach (var f in filters)
			{
				_filters.Add(f);
			}

			SelectedFilter = filters[0];
		}

		#endregion
	}
}
