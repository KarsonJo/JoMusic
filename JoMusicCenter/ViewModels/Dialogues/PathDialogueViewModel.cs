using JoMusicCenter.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JoMusicCenter.ViewModels
{
    /// <summary>
    /// 除了主会话输入以外
    /// 提供一个额外的、输入虚拟路径的输入框数据
    /// </summary>
    public abstract class PathDialogueViewModel : DialogueViewModel
    {
        protected InputHelper pathInput = new();
        public override bool Valid => base.Valid && pathInput.Valid;

        protected List<string>? hierarchicalLocation;

        protected List<string>? defaultLocation;

        public string DefaultLocation
        {
            get
            {
                if (defaultLocation != null && defaultLocation.Count > 0)
                {
                    return string.Join("\\", defaultLocation);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public string PathText
        {
            get => pathInput.InputText;
            set
            {
                pathInput.InputText = value;
            }
        }

        public PathDialogueViewModel()
        {
            pathInput.InputChanged += () =>
            {
                OnPropertyChanged(nameof(PathText));
                TextInputChanged();
                OnPropertyChanged(nameof(Valid));
            };
        }

        private string? pathNotification;

        public string? PathNotification
        {
            get => pathNotification;
            set
            {
                pathNotification = value;
                OnPropertyChanged(nameof(PathNotification));
            }
        }

        protected virtual void TextInputChanged()
        {
            //主要是检测非空路径
            if (pathInput.Valid)
            {
                //转换路径
                hierarchicalLocation = new(PathText.Split('\\'));
            }
            else
            {
                PathNotification = $"输入无效";
                return;
            }

            //显示
            if (hierarchicalLocation != null && hierarchicalLocation.Count > 0)
            {
                PathNotification = $"已选择如下位置：{string.Join(" -> ", hierarchicalLocation)}";
            }
        }

        private ICommand? resetPathCommand;

        public ICommand ResetPathCommand
        {
            get
            {
                if (resetPathCommand == null)
                {
                    resetPathCommand = new RelayCommand(null,
                        p =>
                        {
                            if (defaultLocation != null)
                            {
                                hierarchicalLocation = new(defaultLocation);
                            }
                        });
                }
                return resetPathCommand;
            }
        }
    }
}
