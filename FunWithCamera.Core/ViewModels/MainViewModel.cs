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
				//var list = new List<FilterViewModel>
				//{
				//	new FilterViewModel(Filter.None),
				//	new FilterViewModel(Filter.Antique),
				//	new FilterViewModel(Filter.AutoEnhance),
				//	new FilterViewModel(Filter.AutoLevels),
				//	new FilterViewModel(Filter.Blend),
				//	new FilterViewModel(Filter.Blur),
				//	new FilterViewModel(Filter.Brightness),
				//	new FilterViewModel(Filter.Cartoon),
				//	new FilterViewModel(Filter.ChromaKey),
				//	new FilterViewModel(Filter.Cartoon),
				//	new FilterViewModel(Filter.Grayscale),
				//	new FilterViewModel(Filter.Negative),
				//	new FilterViewModel(Filter.Antique),
				//	new FilterViewModel(Filter.Paint),
				//	new FilterViewModel(Filter.Sepia),
				//	new FilterViewModel(Filter.Sketch),
				//	new FilterViewModel(Filter.Warp)
				//};

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
