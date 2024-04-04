using URManager.View.ViewModel;

namespace URManager.View.Model
{
    public abstract class TabItems : ViewModelBase
    {
        public TabItems(object name, object icon, bool isClosable)
        {
            Name = name;
            Icon = icon;
            IsClosable = isClosable;
        }
        public object Name { get;}
        public object Icon { get;}
        public bool IsClosable { get;}
    }
}
