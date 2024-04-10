using Microsoft.UI.Xaml.Controls;
using URManager.View.Command;

namespace URManager.View.View
{
    public sealed partial class RobotView : Page
    {
        public RobotView()
        {
            this.InitializeComponent();
            MoveNavigationCommand = new DelegateCommand(MoveNavigation);
        }
        public DelegateCommand MoveNavigationCommand { get; }

        /// <summary>
        /// swaps robot view list to other column only for UI necessary
        /// </summary>
        /// <param name="parameter"></param>
        private void MoveNavigation(object parameter)
        {
            var column = Grid.GetColumn(RobotGridList);
            var newColumn = column == 0 ? 2 : 0;
            Grid.SetColumn(RobotGridList, newColumn);

            symbolIconMoveNavigation.Symbol = newColumn == 0 ? Symbol.Forward : Symbol.Back;
        }
    }
}
