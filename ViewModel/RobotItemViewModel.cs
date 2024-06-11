using System.Net;
using URManager.Backend.Model;
using URManager.Backend.ViewModel;

namespace URManager.View.ViewModel
{
    public class RobotItemViewModel : ViewModelBase
    {
        private readonly Robot _model;

        public RobotItemViewModel(Robot model)
        {
            _model = model;
        }

        public int Id => _model.Id;

        public string RobotName
        {
            get => _model.RobotName;
            set
            {
                if (value == _model.RobotName) return;
                _model.RobotName = value;
                RaisePropertyChanged(nameof(RobotName));
            }
        }

        public string IP
        {
            get => _model.IP;
            set
            {
                if (value == _model.IP) return;
                _model.IP = value;
                RaisePropertyChanged(nameof(IP));
            }
        }
    }
}
