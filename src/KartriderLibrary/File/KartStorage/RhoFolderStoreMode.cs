using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartLibrary.File
{
    /// <summary>
    /// The modes of storing a <see cref="RhoArchive"/> when using <see cref="KartStorageSystem"/>.
    /// </summary>
    public enum RhoFolderStoreMode
    {
        /// <summary>
        /// This option only available if this folder has parent folder.
        /// </summary>
        None,
        /// <summary>
        /// Target folder will be stored as PackFolder.
        /// <seealso cref="KartStorageSystem"/> 
        /// </summary>
        PackFolder,
        
        /// <summary>
        /// Target folder will be stored as RhoFile. <br/>
        /// This option only available if the parent of this folder is PackFolder. 
        /// </summary>
        RhoRoot,
        
        /// <summary>
        /// Target folder will be stored as Rho5File. <br/>
        /// This option only available if the parent of this folder is root folder. 
        /// </summary>
        Rho5Root
    }
}
