using SmartClipboard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartClipboard.Utilities
{
    public interface IDatabaseService
    {
        void InsertClipboardItem(ClipboardItem item);
        List<ClipboardItem> GetAllItems();
        void UpdateClipboardItem(ClipboardItem item);
        void DeleteClipboardItem(ClipboardItem item);
        void ClearAllItems();
        void VacuumDatabase();
    }
}
