using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URManager.Backend.Model;
using URManager.Backend.ViewModel;
using URManager.View.ViewModel;

namespace URManager.View.ViewModel
{
    public class FlexibleEthernetIpByteViewModel : ViewModelBase
    {
        private int _byteIndex;
        public ObservableCollection<FlexibleEthernetIpBitViewModel> Bits { get; }
        public FlexibleEthernetIpBytes ByteModel { get; }

        public int ByteIndex
        {
            get => _byteIndex;
            set
            {
                if (_byteIndex != value)
                {
                    _byteIndex = value;
                    RaisePropertyChanged(nameof(ByteIndex));
                }
            }
        }

        public FlexibleEthernetIpByteViewModel(int byteIndex, FlexibleEthernetIpBytes byteModel)
        {
            ByteIndex = byteIndex;
            ByteModel = byteModel;
            Bits = new ObservableCollection<FlexibleEthernetIpBitViewModel>();

            foreach (var bit in byteModel.Bits)
            {
                Bits.Add(new FlexibleEthernetIpBitViewModel(bit));
            }
        }
    }
}
