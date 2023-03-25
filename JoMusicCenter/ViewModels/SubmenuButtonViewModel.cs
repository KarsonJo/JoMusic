using JoMusicCenter.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JoMusicCenter.ViewModels
{
    public class SubmenuButtonViewModel
    {
        public string? Name { get; set; }
        public string? IconFont { get; set; }


        public Func<IEnumerable<FileItemViewModel>, Task>? ButtonClick { get; set; }

        private ICommand? subMenuCommand;
        public ICommand SubMenuCommand
        {
            get
            {
                if (subMenuCommand == null)
                {
                    subMenuCommand = new RelayCommand(
                        null,
                        async p =>
                        {
                            if (p is not System.Collections.IList fileViews)
                            {
                                return;
                            }

                            if (ButtonClick != null)
                            {
                                await ButtonClick(fileViews.Cast<FileItemViewModel>());
                            }

                        });
                }
                return subMenuCommand;
            }
        }
    }
}
