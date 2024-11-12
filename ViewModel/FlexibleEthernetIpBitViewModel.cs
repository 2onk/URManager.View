using URManager.Backend.Model;
using URManager.Backend.ViewModel;

namespace URManager.View.ViewModel
{
    public class FlexibleEthernetIpBitViewModel : ViewModelBase
    {
        private readonly FlexibleEthernetIpBit _model;

        public FlexibleEthernetIpBitViewModel(FlexibleEthernetIpBit model)
        {
            _model = model;
        }
        public int BitIndex => _model.BitIndex;
        public string BitName
        {
            get => _model.BitName;
            set
            {
                if (value == _model.BitName) return;
                _model.BitName = value;
                RaisePropertyChanged(nameof(BitName));
            }
        }
    }
}
