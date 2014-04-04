﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace CSScriptNpp.Dialogs
{
    public partial class DebugObjectsPanel : Form
    {
        private System.Windows.Forms.TextBox editBox = new System.Windows.Forms.TextBox();
        private ListViewItem focucedItem;
        private int X = 0;
        private int Y = 0;

        public DebugObjectsPanel()
        {
            InitializeComponent();

            //listView1.Enabled = true;
            listView1.MouseDoubleClick += listView1_MouseDoubleClick;
            listView1.MouseDown += listView1_MouseDown;

            editBox.Size = new System.Drawing.Size(0, 0);
            editBox.Location = new System.Drawing.Point(0, 0);
            this.Controls.AddRange(new System.Windows.Forms.Control[] { this.editBox });
            editBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.EditOver);
            editBox.LostFocus += new System.EventHandler(this.FocusOver);
            editBox.Font = listView1.Font;
            editBox.BackColor = Color.LightYellow;
            editBox.BorderStyle = BorderStyle.Fixed3D;
            editBox.Hide();
            editBox.Text = "";
        }

        private string subItemText;
        private int subItemSelected = 0;

        void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Check the subitem clicked .
            int nStart = X;
            int spos = 0;
            int epos = listView1.Columns[0].Width;
            for (int i = 0; i < listView1.Columns.Count; i++)
            {
                if (nStart > spos && nStart < epos)
                {
                    subItemSelected = i;
                    break;
                }

                spos = epos;
                epos += listView1.Columns[i].Width;
            }

            subItemText = focucedItem.SubItems[subItemSelected].Text;

            string colName = listView1.Columns[subItemSelected].Text;

            Rectangle r = new Rectangle(spos, focucedItem.Bounds.Y, epos, focucedItem.Bounds.Bottom);
            editBox.Size = new System.Drawing.Size(epos - spos, focucedItem.Bounds.Bottom - focucedItem.Bounds.Top);
            editBox.Location = new System.Drawing.Point(spos, focucedItem.Bounds.Y);
            editBox.Show();
            editBox.Text = subItemText;
            editBox.SelectAll();
            editBox.Focus();
        }

        private void EditOver(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                focucedItem.SubItems[subItemSelected].Text = editBox.Text;
                editBox.Hide();
            }

            if (e.KeyChar == 27)
                editBox.Hide();
        }

        private void FocusOver(object sender, System.EventArgs e)
        {
            focucedItem.SubItems[subItemSelected].Text = editBox.Text;
            editBox.Hide();
        }

        public void SetData(string data)
        {
            ResetWatchObjects(ToWatchObjects(data));
        }

        DbgObject[] ToWatchObjects(string data)
        {
            if (string.IsNullOrEmpty(data))
                return new DbgObject[0];

            var root = XElement.Parse(data);

            var values = root.Elements().Select(dbgValue =>
            {
                string valName = dbgValue.Attribute("name").Value;

                if (valName.EndsWith("__BackingField")) //ignore auto-property backing fields
                    return null;

                string id = dbgValue.Attribute("id").Value;
                bool isArray = dbgValue.Attribute("isArray").Value == "true";
                bool isStatic = dbgValue.Attribute("isStatic").Value == "true";
                bool isField = dbgValue.Attribute("isProperty").Value == "false";
                bool isComplex = dbgValue.Attribute("isComplex").Value == "true";
                string type = dbgValue.Attribute("typeName").Value;

                var dbgObject = new DbgObject();
                dbgObject.DbgId = id;
                dbgObject.Name = valName;
                dbgObject.Type = type;
                dbgObject.IsArray = isArray;
                dbgObject.HasChildren = isComplex;
                dbgObject.IsField = isField;
                dbgObject.IsStatic = isStatic;

                if (!isComplex)
                {
                    // This is a catch-all for primitives.
                    string stValue = dbgValue.Attribute("value").Value;
                    dbgObject.Value = stValue;
                }

                return dbgObject;

            }).Where(x => x != null);

            var staticMembers = values.Where(x => x.IsStatic);
            var instanceMembers = values.Where(x => !x.IsStatic);
            var result = new List<DbgObject>(instanceMembers);
            if (staticMembers.Any())
                result.Add(
                    new DbgObject
                    {
                        Name = "Static members",
                        HasChildren = true,
                        IsSeparator = true,
                        Children = staticMembers.ToArray()
                    });
            return result.ToArray();
        }

        public void AddWatchObject(DbgObject item)
        {
            listView1.Items.Add(item.ToListViewItem());
        }

        public void ResetWatchObjects(params DbgObject[] items)
        {
            listView1.Items.Clear();

            foreach (var item in items.ToListViewItems())
                listView1.Items.Add(item);
        }

        void InsertWatchObjects(int index, params DbgObject[] items)
        {
            if (index == listView1.Items.Count)
                listView1.Items.AddRange(items.ToListViewItems().ToArray());
            else
                foreach (var item in items.ToListViewItems().Reverse())
                    listView1.Items.Insert(index, item);
        }

        int triangleMargin = 8;

        Range GetItemExpenderClickableRange(ListViewItem item)
        {
            var dbgObject = (DbgObject)item.Tag;

            int xOffset = 10 * (dbgObject.IndentationLevel); //depends on the indentation
            return new Range { Start = xOffset, End = xOffset + triangleMargin };
        }

        private void listView1_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.DrawBackground();

            var dbgObject = (DbgObject)e.Item.Tag;

            var textBrush = Brushes.Black;
            if (!Debugger.IsInBreak)
                textBrush = Brushes.DarkGray;

            if (e.ColumnIndex == 0)
            {

                var clickableRange = GetItemExpenderClickableRange(e.Item);
                int xMargin = 27; //fixed; to accommodate the icon

                int X = e.Bounds.X + clickableRange.Start;
                int Y = e.Bounds.Y;

                var range = GetItemExpenderClickableRange(e.Item);

                Image icon;

                if (dbgObject.IsSeparator)
                    icon = Resources.Resources.dbg_container;
                else if (dbgObject.IsField || dbgObject.IsExpression)
                    icon = Resources.Resources.field;
                else
                    icon = Resources.Resources.property;

                e.Graphics.DrawImage(icon, range.End + 6, e.Bounds.Y);

                if (dbgObject.HasChildren)
                {
                    int xOffset;
                    int triangleWidth;
                    int triangleHeight;
                    int yOffset;

                    if (dbgObject.IsExpanded)
                    {
                        xOffset = 0;
                        triangleWidth = 7;
                        triangleHeight = 7;
                        yOffset = (e.Bounds.Height - triangleHeight) / 2;

                        e.Graphics.FillPolygon(Brushes.Black, new[]
                        {
                            new Point(X + xOffset + triangleWidth, Y + yOffset),
                            new Point(X + xOffset + triangleWidth, Y + triangleHeight + yOffset),
                            new Point(X + xOffset + triangleWidth - triangleWidth, Y +triangleHeight + yOffset),
                        });
                    }
                    else
                    {
                        xOffset = 2;
                        triangleWidth = 4;
                        triangleHeight = 8;
                        yOffset = (e.Bounds.Height - triangleHeight) / 2;

                        e.Graphics.DrawPolygon(Pens.Black, new[]
                        {
                            new Point(X + xOffset, Y + yOffset),
                            new Point(X + xOffset + triangleWidth, Y + triangleHeight / 2 + yOffset),
                            new Point(X + xOffset,Y + triangleHeight + yOffset),
                        });
                    }
                }

                int textStartX = X + triangleMargin + xMargin;

                if (e.Item.Selected)
                {
                    var rect = e.Bounds;
                    rect.Inflate(-1, -1);
                    rect.Offset(textStartX - 5, 0);
                    e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                }

                e.Graphics.DrawString(e.Item.Text, listView1.Font, textBrush, textStartX, Y);
                SizeF size = e.Graphics.MeasureString(e.Item.Text, listView1.Font);

                int requiredWidth = textStartX + (int)size.Width;
                if (e.Bounds.Width < requiredWidth)
                {
                    listView1.Columns[0].Width = requiredWidth + 5;
                }
            }
            else
            {
                if (e.Item.Selected)
                {
                    var rect = e.Bounds;
                    rect.Inflate(-1, -1);
                    e.Graphics.FillRectangle(Brushes.LightBlue, rect);
                }

                SizeF size = e.Graphics.MeasureString(e.SubItem.Text, listView1.Font);
                int requiredWidth = Math.Max(30, (int)size.Width + 20);
                if (listView1.Columns[e.ColumnIndex].Width < requiredWidth)
                {
                    listView1.Columns[e.ColumnIndex].Width = requiredWidth + 5;
                }
                e.Graphics.DrawString(e.SubItem.Text, listView1.Font, textBrush, e.Bounds);
                //e.DrawText();
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            focucedItem = listView1.GetItemAt(e.X, e.Y);
            X = e.X;
            Y = e.Y;
            
            ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
            if (info.Item != null)
            {
                Range clickableRange = GetItemExpenderClickableRange(info.Item);
                if (clickableRange.Contains(e.X))
                {
                    OnExpandItem(info.Item);
                }
            }
        }

        void OnExpandItem(ListViewItem item)
        {
            var dbgObject = (item.Tag as DbgObject);
            if (dbgObject.HasChildren)
            {
                if (dbgObject.IsExpanded)
                {
                    foreach (var c in listView1.LootupListViewItems(dbgObject.Children))
                        listView1.Items.Remove(c);
                }
                else
                {
                    if (dbgObject.Children == null)
                    {
                        string data = Debugger.Invoke("locals", dbgObject.DbgId);
                        dbgObject.Children = ToWatchObjects(data);
                        dbgObject.HasChildren = dbgObject.Children.Any(); //readjust as complex type (e.g. array) may not have children after the deep inspection 
                        if (dbgObject.IsArray)
                        {
                            dbgObject.Value = string.Format("[{0} items]", dbgObject.Children.Count());
                            item.SubItems[1].Text = dbgObject.Value;
                        }
                    }

                    int index = listView1.IndexOfObject(dbgObject);
                    if (index != -1)
                    {
                        InsertWatchObjects(index + 1, dbgObject.Children);
                    }
                }
                dbgObject.IsExpanded = !dbgObject.IsExpanded;
                listView1.Invalidate();
            }
        }

        private ListViewItem GetItemFromPoint(ListView listView, Point mousePosition)
        {
            // translate the mouse position from screen coordinates to
            // client coordinates within the given ListView
            Point localPoint = listView.PointToClient(mousePosition);
            return listView.GetItemAt(localPoint.X, localPoint.Y);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    var buffer = new StringBuilder();

                    foreach (ListViewItem item in listView1.SelectedItems)
                    {
                        var dbgObject = item.Tag as DbgObject;
                        buffer.AppendLine(string.Format("{0} {1} {2}", dbgObject.Name, dbgObject.Value, dbgObject.Type));
                    }
                    Clipboard.SetText(buffer.ToString());

                }
                catch { }
            }
        }

        public bool IsReadOnly = true;

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (!IsReadOnly && e.Data.GetDataPresent(DataFormats.StringFormat))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (OnDagDropText != null)
                OnDagDropText((string)e.Data.GetData(DataFormats.StringFormat));
        }

        public event Action<string> OnDagDropText;

        private void listView1_MouseDoubleClick_1(object sender, MouseEventArgs e)
        {
            MessageBox.Show("DoubleClick");
        }
    }

    public class Range
    {
        public int Start { get; set; }
        public int End { get; set; }
        public int Width { get { return End - Start; } }
        public bool Contains(int point)
        {
            return Start < point && point < End;
        }
    }

    public class DbgObject
    {
        DbgObject[] children;

        public DbgObject[] Children
        {
            get { return children; }
            set
            {
                children = value;
                if (Children != null)
                    Array.ForEach(Children, x => x.Parent = this);
            }
        }
        public DbgObject Parent { get; set; }
        public bool HasChildren { get; set; }
        public bool IsExpanded { get; set; }
        public bool IsStatic { get; set; }
        public bool IsArray { get; set; }
        public bool IsField { get; set; }
        public string DbgId { get; set; }
        public bool IsPublic { get; set; }
        public bool IsSeparator { get; set; }
        public bool IsExpression { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public int IndentationLevel
        {
            get
            {
                int level = 0;
                DbgObject parent = this.Parent;
                while (parent != null)
                {
                    level++;
                    parent = parent.Parent;
                }
                return level;
            }
        }
    }

    static class Extensions
    {
        public static T[] AllNestedItems<T>(this T item, Func<T, IEnumerable<T>> getChildren)
        {
            int iterator = 0;
            var elementsList = new List<T>();
            var allElements = new List<T>();

            elementsList.Add(item);

            while (iterator < elementsList.Count)
            {
                foreach (T e in getChildren(elementsList[iterator]))
                {
                    elementsList.Add(e);
                    allElements.Add(e);
                }

                iterator++;
            }

            return allElements.ToArray();
        }

        public static IEnumerable<ListViewItem> ToListViewItems(this IEnumerable<DbgObject> items)
        {
            return items.Select(x => x.ToListViewItem());
        }

        public static ListViewItem ToListViewItem(this DbgObject item)
        {
            string name = item.Name;

            var li = new ListViewItem(name);
            li.SubItems.Add(item.Value);
            li.SubItems.Add(item.Type);
            li.Tag = item;
            return li;
        }

        public static IEnumerable<ListViewItem> LootupListViewItems(this ListView listView, IEnumerable<DbgObject> items)
        {
            return listView.Items.Cast<ListViewItem>().Where(x => items.Contains(x.Tag as DbgObject));
        }

        public static int IndexOfObject(this ListView listView, DbgObject item)
        {
            for (int i = 0; i < listView.Items.Count; i++)
            {
                if (listView.Items[i].Tag == item)
                    return i;
            }
            return -1;
        }

        public static TabControl AddTab(this TabControl control, string tabName, Form content)
        {
            var page = new TabPage
            {
                Padding = new System.Windows.Forms.Padding(3),
                TabIndex = control.TabPages.Count,
                Text = tabName,
                UseVisualStyleBackColor = true
            };

            control.Controls.Add(page);

            content.TopLevel = false;
            content.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            content.Parent = page;
            page.Controls.Add(content);
            content.Dock = DockStyle.Fill;
            content.Visible = true;

            return control;
        }
    }
}
