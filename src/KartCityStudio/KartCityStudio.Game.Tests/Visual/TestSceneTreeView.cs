using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KartCityStudio.Game.Graphics.UserInterface;
using KartLibrary.File;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;

namespace KartCityStudio.Game.Tests.Visual
{
    [TestFixture]
    public partial class TestSceneTreeView : KartCityStudioTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.
        private KCSTreeView treeView;

        private TreeViewNode firstNode;

        [Resolved]
        private KartStorageSystem storageSystem { get; set; }

        public TestSceneTreeView()
        {
            Add(treeView = new KCSTreeView()
            {
                RelativeSizeAxes = Axes.Both,
                BackgroundColour = Colour4.Black,
            });

            firstNode = new TreeViewNode($"TestItem1-1")
            {
                Nodes =
                {
                    new TreeViewNode($"TestItem1-1-1"),
                    new TreeViewNode($"TestItem1-1-2"),
                }
            };

            AddStep("Add 10 items to first node.", () =>
            {
                for (int i = 0; i < 10; i++)
                    firstNode.Nodes.Add(new TreeViewNode($"TestItem1-1-{firstNode.Nodes.Count + 1}"));
            });

            AddStep("Show KartRider Storage System files.", buildTree);

            treeView.Nodes.Add(new TreeViewNode($"TestItem1")
            {
                Nodes =
                {
                    firstNode,
                    new TreeViewNode($"TestItem1-2"),
                }
            });
            treeView.Nodes.Add(new TreeViewNode($"TestItem2"));
        }

        private void buildTree()
        {
            treeView.Nodes.Clear();
            Queue<Tuple<TreeViewNode, KartStorageFolder>> bfsQueue = new Queue<Tuple<TreeViewNode, KartStorageFolder>>();
            bfsQueue.Enqueue(new (null, storageSystem.RootFolder));
            while (bfsQueue.Count > 0)
            {
                var dequeueEle = bfsQueue.Dequeue();
                foreach (KartStorageFolder folder in dequeueEle.Item2.Folders)
                {
                    TreeViewNode childFolderNode = new TreeViewNode(
                        text: folder.Name,
                        iconName: "folder_close",
                        tag: folder.Name
                    );
                    bfsQueue.Enqueue(new(childFolderNode, folder));
                    if (dequeueEle.Item1 is not null)
                    {
                        dequeueEle.Item1.Nodes.Add(childFolderNode);
                    }
                    else
                    {
                        treeView.Nodes.Add(childFolderNode);
                    }
                }

                foreach (KartStorageFile file in dequeueEle.Item2.Files)
                {
                    string ext = file.Name.Length > 4 ? file.Name[^4..] : "";
                    string iconName = ext switch
                    {
                        ".png" => "file_image",
                        ".jpg" => "file_image",
                        ".tgs" => "file_image",
                        ".dds" => "file_image",
                        ".jpeg" => "file_image",
                        ".ksv" => "file_ksv",
                        ".ogg" => "file_music",
                        ".xml" => "file_xml",
                        ".bml" => "file_xml",
                        _ => "object_unknown",
                        // _ => ""
                    };
                    TreeViewNode childFileNode = new TreeViewNode(
                        text: file.Name,
                        iconName: iconName,
                        tag: file.Name
                    );
                    if (dequeueEle.Item1 is not null)
                    {
                        dequeueEle.Item1.Nodes.Add(childFileNode);
                    }
                    else
                    {
                        treeView.Nodes.Add(childFileNode);
                    }
                }
            }
        }
    }


}
