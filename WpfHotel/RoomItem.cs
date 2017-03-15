using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfHotel
{
    public class RoomItem
    {
        public Room Room { get; set; }

        public ImageSource Image
        {
            get
            {
                string[] images={"","kong.png","di.png","zhu.png","zang.png","xiu.png","ting,png","li.png"};
                return new BitmapImage(new Uri("pack://application:,,,/WpfHotel;component/img/" + images[Room.Status.Value]));
            }
        }

        public ContextMenu ContextMenu
        {
            get
            {
                ContextMenu contextMenu=new ContextMenu();
                switch (Room.Status)
                {
                    case 1:
                        MenuItem checkInMenuItem = new MenuItem {Header = "办理入住"};
                        checkInMenuItem.Click += CheeckIn;
                        contextMenu.Items.Add(checkInMenuItem);
                        break;
                    default:
                        break;
                }
                return contextMenu;
            }
        }

        private void CheeckIn(object sender, RoutedEventArgs e)
        {
            CheckInWindow checkInWindow=new CheckInWindow();
            checkInWindow.ShowDialog();
        }

        private ImageSource _image;
        private ContextMenu _contextMenu;

        
    }
}
