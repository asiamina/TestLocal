using VSIXProjectThesis.Model;

namespace VSIXProjectThesis.Model
{
    public class ViewModelLocator
    {
        private static MainModel mainModel = null;

        public MainModel MainModel
        {
            get
            {
                return mainModel ?? (mainModel = new MainModel());
            }
        }

       

    }
}
