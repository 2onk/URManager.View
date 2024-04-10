using System.Net;
using URManager.Backend.Model;

namespace URManager.View.ViewModel
{
    public class RobotItemViewModel : ValidationViewModelBase
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
                //if (string.IsNullOrEmpty(_model.RobotName))
                //{
                //    AddError("Robotname is required");
                //}
                //else
                //{
                //    ClearErrors();
                //}
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
                //if (string.IsNullOrEmpty(_model.IP))
                //{
                //    AddError("IP is required");
                //}
                //else if (!IPAddress.TryParse(_model.IP, out _))
                //{
                //    AddError("IPv4 format is required");
                //}
                //else
                //{
                //    ClearErrors();
                //}
            }
        }
    }
}
