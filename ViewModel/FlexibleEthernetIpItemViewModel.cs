using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using URManager.Backend.Model;
using URManager.Backend.ViewModel;

namespace URManager.View.ViewModel
{
    public class FlexibleEthernetIpItemViewModel : ViewModelBase
    {
        public ObservableCollection<FlexibleEthernetIpByteViewModel> Bytes { get; }

        private FlexibleEthernetIpByteViewModel _selectedByte;
        public FlexibleEthernetIpByteViewModel SelectedByte
        {
            get => _selectedByte;
            set
            {
                _selectedByte = value;
                RaisePropertyChanged(nameof(SelectedByte));
            }
        }

        public FlexibleEthernetIpItemViewModel()
        {
            Bytes = new ObservableCollection<FlexibleEthernetIpByteViewModel>();
            AddByte();
        }

        public void AddByte()
        {
            var byteModel = new FlexibleEthernetIpBytes();
            Bytes.Add(new FlexibleEthernetIpByteViewModel(Bytes.Count, byteModel));
        }

        public List<FlexibleEthernetIpBytes> ToFlexibleEthernetIpBytesList()
        {
            return Bytes
                .Select(byteViewModel => byteViewModel.ByteModel) // ByteModel ist das FlexibleEthernetIpBytes-Objekt in jedem ViewModel
                .ToList();
        }


        //public void DeleteByteBySelectedBit(FlexibleEthernetIpBitViewModel selectedBit)
        //{
        //    var byteToDelete = Bytes.FirstOrDefault(b => b.Bits.Contains(selectedBit));
        //    if (byteToDelete != null)
        //    {
        //        Bytes.Remove(byteToDelete);

        //        for (int i = 0; i < Bytes.Count; i++)
        //        {
        //            Bytes[i].ByteIndex = i;
        //        }
        //    }
        //}
    }
}
