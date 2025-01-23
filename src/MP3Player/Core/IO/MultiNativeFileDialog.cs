using System.Linq;
using Terraria.Utilities.FileBrowser;

namespace MP3Player.Core.IO
{
    public class MultiNativeFileDialog
    {
        public string[] OpenFilePanelMulti(ExtensionFilter[] extensions)
        {
            string[] value = extensions.SelectMany((ExtensionFilter x) => x.Extensions).ToArray();

            if (nativefiledialog.NFD_OpenDialogMultiple(string.Join(",", value), null, out var outPaths) == nativefiledialog.nfdresult_t.NFD_OKAY)
            {
                int count = (int)nativefiledialog.NFD_PathSet_GetCount(ref outPaths);

                string[] result = new string[count];

                for (int i = 0; i < count; i++)
                {
                    result[i] = nativefiledialog.NFD_PathSet_GetPath(ref outPaths, (nint)i);
                }

                nativefiledialog.NFD_PathSet_Free(ref outPaths);

                return result;
            }

            return null;
        }
    }
}
