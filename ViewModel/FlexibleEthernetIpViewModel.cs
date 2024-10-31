using URManager.Backend.FlexibleEthernetIp;
using URManager.Backend.Model;
using URManager.Backend.ViewModel;
using URManager.View.Command;

namespace URManager.View.ViewModel
{
    public class FlexibleEthernetIpViewModel : TabItems
    {
        private FlexibleEthernetIpByteViewModel _selectedInputByte;
        private FlexibleEthernetIpByteViewModel _selectedOutputByte;
        public FlexibleEthernetIpItemViewModel Inputs { get; set; } = new FlexibleEthernetIpItemViewModel();
        public FlexibleEthernetIpItemViewModel Outputs { get; set; } = new FlexibleEthernetIpItemViewModel();
        public FlexibleEthernetIpViewModel(object name, object icon, bool isClosable) : base(name, icon, isClosable)
        {
            AddInputByteCommand = new DelegateCommand(AddInputByte);
            AddOutputByteCommand = new DelegateCommand(AddOutputByte);
            DeleteInputByteCommand = new DelegateCommand(DeleteInputByte, CanDeleteInputByte);
            DeleteOutputByteCommand = new DelegateCommand(DeleteOutputByte, CanDeleteOutputByte);
            GenerateScriptCommand = new DelegateCommand(GenerateScript);
        }

        public DelegateCommand AddInputByteCommand { get; }
        public DelegateCommand AddOutputByteCommand { get; }
        public DelegateCommand DeleteInputByteCommand { get; }
        public DelegateCommand DeleteOutputByteCommand { get; }
        public DelegateCommand GenerateScriptCommand { get; }

        public FlexibleEthernetIpByteViewModel SelectedInputByte
        {
            get => _selectedInputByte;
            set
            {
                if (value == _selectedInputByte) return;
                _selectedInputByte = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsInputByteSelected));
                DeleteInputByteCommand.RaiseCanExecuteChanged();
            }
        }

        public FlexibleEthernetIpByteViewModel SelectedOutputByte
        {
            get => _selectedOutputByte;
            set
            {
                if (value == _selectedOutputByte) return;
                _selectedOutputByte = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsOutputByteSelected));
                DeleteOutputByteCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Get bool if any Inputbyte in list is selected
        /// </summary>
        public bool IsInputByteSelected => SelectedInputByte is not null ;

        /// <summary>
        /// Get bool if any Outputbyte in list is selected
        /// </summary>
        public bool IsOutputByteSelected => SelectedOutputByte is not null;

        /// <summary>
        /// Add new input byte to Flexible EthernetIp list
        /// </summary>
        /// <param name="parameter"></param>
        private void AddInputByte(object parameter)
        {
            Inputs.AddByte();
        }

        /// <summary>
        /// Add new output byte to Flexible EthernetIp list
        /// </summary>
        /// <param name="parameter"></param>
        private void AddOutputByte(object parameter)
        {
            Outputs.AddByte();
        }
        
        /// <summary>
        /// Delete selected Byte Input in listview
        /// </summary>
        /// <param name="parameter"></param>
        private void DeleteInputByte(object parameter)
        {
            if (SelectedInputByte is null) return;

            Inputs.Bytes.Remove(SelectedInputByte);

            SelectedInputByte = null;

            for (int i = 0; i < Inputs.Bytes.Count; i++)
            {
                Inputs.Bytes[i].ByteIndex = i;
            }
        }

        /// <summary>
        /// Delete selected Byte Input in listview
        /// </summary>
        /// <param name="parameter"></param>
        private void DeleteOutputByte(object parameter)
        {
            if (SelectedOutputByte is null) return;

            Outputs.Bytes.Remove(SelectedOutputByte);

            SelectedOutputByte = null;

            for (int i = 0; i < Outputs.Bytes.Count; i++)
            {
                Outputs.Bytes[i].ByteIndex = i;
            }
        }

        /// <summary>
        /// If Inputbyte is selected activate delete possible
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>bool</returns>
        private bool CanDeleteInputByte(object parameter) => SelectedInputByte is not null;

        /// <summary>
        /// If Outputbyte is selected activate delete possible
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>bool</returns>
        private bool CanDeleteOutputByte(object parameter) => SelectedOutputByte is not null;


        private async void GenerateScript(object parameter)
        {
            var scriptGenerator = new FlexibleEthernetIpScriptGenerator(Inputs.ToFlexibleEthernetIpBytesList(), Outputs.ToFlexibleEthernetIpBytesList());
            string savepath = await FilePicker.SaveAsync("Choose your saving path", ".script", "FlexibleEthernetIp.script");

            scriptGenerator.SaveScriptToFile(savepath);
        }
    }
}
