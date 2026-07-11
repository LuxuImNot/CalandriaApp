using System;
using System.Windows.Forms;

public partial class FormEditorTreeList : Form
{
    private NodoTree nodoTree;

    public FormEditorTreeList()
    {
        InitializeComponent();

        // Create an instance of the NodoTree class
        nodoTree = new NodoTree();

        // Link the TreeView events with the methods of the NodoTree class

        // When a node is clicked, call the OnNodeClick method of the NodoTree class
        treeView1.NodeMouseClick += (sender, e) => nodoTree.OnNodeClick(e.Node);

        // When a node is expanded, call the OnNodeExpand method of the NodoTree class
        treeView1.AfterExpand += (sender, e) => nodoTree.OnNodeExpand(e.Node);
    }
}
