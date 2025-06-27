using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BFSVisualizer
{
    public partial class Form1 : Form
    {
        class TreeNode
        {
            public int Value;
            public List<TreeNode> Children = new List<TreeNode>();
            public Point Position;
            public TreeNode(int val) => Value = val;
        }

        TreeNode? root;
        TreeNode? bfsRoot;
        TreeNode? dfsRoot;
        List<TreeNode> traversalOrder = new List<TreeNode>();
        int traversalIndex = 0;
        Queue<TreeNode> currentQueue = new Queue<TreeNode>();
        Stack<TreeNode> currentStack = new Stack<TreeNode>();
        HashSet<TreeNode> visitedNodes = new HashSet<TreeNode>();
        bool isSimulating = false;
        bool isDFS = false;
        Button btnStart = null!, btnNext = null!, btnReset = null!, btnCreateGraph = null!;
        ComboBox cmbAlgorithm = null!;
        TextBox txtGraphInput = null!;
        Label lblGraphInput = null!;

        public Form1()
        {
            InitializeComponent();
            this.Width = 1200;
            this.Height = 700;
            this.DoubleBuffered = true;
            this.KeyPreview = true; // Allow form to receive key events
            this.KeyDown += Form1_KeyDown; // Connect the event handler
            this.WindowState = FormWindowState.Maximized; // Start maximized
            this.Resize += Form1_Resize; // Handle resize events

            CreateControls();
            BuildTree();
            ResetSimulation();
        }

        private void CreateControls()
        {
            // Create algorithm selection dropdown
            cmbAlgorithm = new ComboBox
            {
                Location = new Point(20, 20),
                Size = new Size(100, 30),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbAlgorithm.Items.Add("BFS");
            cmbAlgorithm.Items.Add("DFS");
            cmbAlgorithm.SelectedIndex = 0; // Default to BFS
            cmbAlgorithm.SelectedIndexChanged += CmbAlgorithm_SelectedIndexChanged;

            // Create buttons
            btnStart = new Button
            {
                Text = "Start BFS",
                Location = new Point(130, 20),
                Size = new Size(100, 40),
                BackColor = Color.LightGreen
            };
            btnStart.Click += BtnStart_Click;

            btnNext = new Button
            {
                Text = "Next Step",
                Location = new Point(240, 20),
                Size = new Size(100, 40),
                BackColor = Color.LightBlue,
                Enabled = false
            };
            btnNext.Click += BtnNext_Click;

            btnReset = new Button
            {
                Text = "Reset",
                Location = new Point(350, 20),
                Size = new Size(100, 40),
                BackColor = Color.LightCoral
            };
            btnReset.Click += BtnReset_Click;

            // Add controls to form
            this.Controls.Add(cmbAlgorithm);
            this.Controls.Add(btnStart);
            this.Controls.Add(btnNext);
            this.Controls.Add(btnReset);
        }

        private void BuildTree()
        {
            // Build BFS Tree (original tree)
            BuildBFSTree();
            
            // Build DFS Tree (new tree structure)
            BuildDFSTree();
            
            // Set initial root
            root = bfsRoot;
        }

        private void BuildBFSTree()
        {
            // Tạo node
            TreeNode[] nodes = new TreeNode[15];
            for (int i = 1; i <= 14; i++)
                nodes[i] = new TreeNode(i);

            // Xây cây BFS (theo hình cũ)
            nodes[1].Children.AddRange(new[] { nodes[2], nodes[3], nodes[4] });
            nodes[2].Children.AddRange(new[] { nodes[5], nodes[6] });
            nodes[3].Children.Add(nodes[7]);
            nodes[4].Children.AddRange(new[] { nodes[8], nodes[9] });
            nodes[5].Children.Add(nodes[10]);
            nodes[6].Children.Add(nodes[11]);
            nodes[8].Children.Add(nodes[12]);
            nodes[9].Children.AddRange(new[] { nodes[13], nodes[14] });

            bfsRoot = nodes[1];
            
            // Set initial positions (will be updated by UpdateNodePositions)
            UpdateBFSNodePositions();
        }

        private void BuildDFSTree()
        {
            // Tạo node cho DFS tree
            TreeNode[] nodes = new TreeNode[10];
            for (int i = 1; i <= 9; i++)
                nodes[i] = new TreeNode(i);

            // Xây cây DFS theo yêu cầu:
            // (1, 2), (1, 6), (1, 8)
            // (2, 3), (2, 4)
            // (3, 5)
            // (6, 7)
            // (8, 9)
            nodes[1].Children.AddRange(new[] { nodes[2], nodes[6], nodes[8] });
            nodes[2].Children.AddRange(new[] { nodes[3], nodes[4] });
            nodes[3].Children.Add(nodes[5]);
            nodes[6].Children.Add(nodes[7]);
            nodes[8].Children.Add(nodes[9]);

            dfsRoot = nodes[1];
            
            // Set initial positions (will be updated by UpdateNodePositions)
            UpdateDFSNodePositions();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 10);
            Font boldFont = new Font("Arial", 12, FontStyle.Bold);
            Pen linePen = new Pen(Color.Black, 2);

            // Vẽ cạnh
            if (root != null)
                DrawEdges(g, root, linePen);

            // Vẽ node
            if (root != null)
                DrawAllNodes(g, font);

            // Draw queue visualization
            DrawQueueVisualization(g, font, boldFont);

            // Draw legend
            DrawLegend(g, font);
        }

        private void DrawAllNodes(Graphics g, Font font)
        {
            DrawNodesRecursively(g, root!, font);
        }

        private void DrawNodesRecursively(Graphics g, TreeNode node, Font font)
        {
            // Determine node color based on state
            Brush brush;
            Pen borderPen;

            if (traversalOrder.Contains(node))
            {
                int nodeIndex = traversalOrder.IndexOf(node);
                if (nodeIndex == traversalIndex && isSimulating)
                {
                    brush = Brushes.Yellow; // Currently processing
                    borderPen = new Pen(Color.Orange, 3);
                }
                else if (nodeIndex <= traversalIndex)
                {
                    brush = Brushes.LightGreen; // Visited
                    borderPen = new Pen(Color.Green, 2);
                }
                else
                {
                    brush = Brushes.White; // Not yet processed
                    borderPen = Pens.Black;
                }
            }
            else
            {
                brush = Brushes.White; // Not visited
                borderPen = Pens.Black;
            }

            // Draw node
            g.FillEllipse(brush, node.Position.X - 20, node.Position.Y - 20, 40, 40);
            g.DrawEllipse(borderPen, node.Position.X - 20, node.Position.Y - 20, 40, 40);
            
            // Draw node value
            string text = node.Value.ToString();
            SizeF textSize = g.MeasureString(text, font);
            g.DrawString(text, font, Brushes.Black, 
                node.Position.X - textSize.Width/2, 
                node.Position.Y - textSize.Height/2);

            // Recursively draw children
            foreach (var child in node.Children)
            {
                DrawNodesRecursively(g, child, font);
            }
        }

        private void DrawQueueVisualization(Graphics g, Font font, Font boldFont)
        {
            int baseX = 20;
            int baseY = this.ClientSize.Height - 500; 
            
            if (isDFS)
            {
                // Stack visualization for DFS
                g.DrawString("Current Stack:", boldFont, Brushes.Black, baseX, baseY - 200);
                
                // Draw stack elements (from bottom to top)
                int x = baseX;
                int y = baseY + 30; // Bottom position of stack
                var stackArray = currentStack.ToArray();
                
                for (int i = stackArray.Length - 1; i >= 0; i--)
                {
                    var node = stackArray[i];
                    int stackIndex = stackArray.Length - 1 - i;
                    
                    // Draw stack box (going upward from bottom) with more spacing
                    Rectangle rect = new Rectangle(x, y - stackIndex * 40, 50, 35);
                    g.FillRectangle(Brushes.LightCoral, rect);
                    g.DrawRectangle(Pens.Red, rect);
                    
                    // Draw node value
                    string text = node.Value.ToString();
                    SizeF textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, Brushes.Black, 
                        rect.X + (rect.Width - textSize.Width)/2,
                        rect.Y + (rect.Height - textSize.Height)/2);
                    
                    // Draw "TOP" label for the top element
                    if (i == 0)
                    {
                        g.DrawString("← TOP", font, Brushes.Red, x + 55, rect.Y + 12);
                    }
                }
                
                // Stack status
                string status = isSimulating ? 
                    $"Stack Size: {currentStack.Count}" : 
                    "Click 'Start DFS' to begin simulation";
                g.DrawString(status, font, Brushes.Black, baseX, baseY + 70);
            }
            else
            {
                // Queue visualization for BFS
                g.DrawString("Current Queue:", boldFont, Brushes.Black, baseX, baseY);
                
                // Draw queue elements
                int x = baseX;
                int y = baseY + 30;
                int index = 0;
                
                foreach (var node in currentQueue)
                {
                    // Draw queue box
                    Rectangle rect = new Rectangle(x + index * 60, y, 50, 40);
                    g.FillRectangle(Brushes.LightBlue, rect);
                    g.DrawRectangle(Pens.Blue, rect);
                    
                    // Draw node value
                    string text = node.Value.ToString();
                    SizeF textSize = g.MeasureString(text, font);
                    g.DrawString(text, font, Brushes.Black, 
                        rect.X + (rect.Width - textSize.Width)/2,
                        rect.Y + (rect.Height - textSize.Height)/2);
                    
                    // Draw arrow (except for last element)
                    if (index < currentQueue.Count - 1)
                    {
                        g.DrawString("→", font, Brushes.Black, x + (index + 1) * 60 - 10, y + 15);
                    }
                    
                    index++;
                }

                // Queue status
                string status = isSimulating ? 
                    $"Queue Size: {currentQueue.Count}" : 
                    "Click 'Start BFS' to begin simulation";
                g.DrawString(status, font, Brushes.Black, baseX, baseY + 90);
            }
        }

        private void DrawLegend(Graphics g, Font font)
        {
            int x = this.ClientSize.Width - 250; // Position from right edge
            int y = this.ClientSize.Height - 120; // Position from bottom (adjusted for fewer items)
            
            g.DrawString("Type:", new Font("Arial", 12, FontStyle.Bold), Brushes.Black, x, y);
            
            // White - Not visited
            g.FillEllipse(Brushes.White, x, y + 25, 20, 20);
            g.DrawEllipse(Pens.Black, x, y + 25, 20, 20);
            g.DrawString("Not Visited", font, Brushes.Black, x + 30, y + 28);
            
            // Yellow - Currently Processing
            g.FillEllipse(Brushes.Yellow, x, y + 50, 20, 20);
            g.DrawEllipse(new Pen(Color.Orange, 3), x, y + 50, 20, 20);
            g.DrawString("Processing", font, Brushes.Black, x + 30, y + 53);
            
            // Light Green - Visited
            g.FillEllipse(Brushes.LightGreen, x, y + 75, 20, 20);
            g.DrawEllipse(new Pen(Color.Green, 2), x, y + 75, 20, 20);
            g.DrawString("Visited", font, Brushes.Black, x + 30, y + 78);
        }

        private void DrawEdges(Graphics g, TreeNode node, Pen pen)
        {
            foreach (var child in node.Children)
            {
                g.DrawLine(pen, node.Position, child.Position);
                DrawEdges(g, child, pen);
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space && btnNext.Enabled)
            {
                BtnNext_Click(sender, e);
            }
            else if (e.KeyCode == Keys.Enter && btnStart.Enabled)
            {
                BtnStart_Click(sender, e);
            }
            else if (e.KeyCode == Keys.R)
            {
                BtnReset_Click(sender, e);
            }
        }

        private void ResetSimulation()
        {
            traversalOrder.Clear();
            currentQueue.Clear();
            currentStack.Clear();
            visitedNodes.Clear();
            traversalIndex = 0;
            isSimulating = false;

            if (root != null)
            {
                if (isDFS)
                {
                    currentStack.Push(root);
                }
                else
                {
                    currentQueue.Enqueue(root);
                }
            }

            btnStart.Enabled = true;
            btnNext.Enabled = false;
            this.Invalidate();
        }

        private void CmbAlgorithm_SelectedIndexChanged(object? sender, EventArgs e)
        {
            isDFS = cmbAlgorithm.SelectedIndex == 1;
            btnStart.Text = isDFS ? "Start DFS" : "Start BFS";
            
            // Switch to appropriate tree
            root = isDFS ? dfsRoot : bfsRoot;
            
            // Update positions for the new tree
            UpdateNodePositions();
            
            ResetSimulation();
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (root != null)
            {
                isSimulating = true;
                btnStart.Enabled = false;
                btnNext.Enabled = true;
                this.Invalidate();
            }
        }

        private void BtnNext_Click(object? sender, EventArgs e)
        {
            if (isDFS)
            {
                // DFS using stack
                if (currentStack.Count > 0 && isSimulating)
                {
                    var node = currentStack.Pop();
                    traversalOrder.Add(node);
                    visitedNodes.Add(node);

                    // Add children to stack (in reverse order to maintain left-to-right traversal)
                    for (int i = node.Children.Count - 1; i >= 0; i--)
                    {
                        var child = node.Children[i];
                        if (!visitedNodes.Contains(child) && !currentStack.Contains(child))
                        {
                            currentStack.Push(child);
                        }
                    }

                    traversalIndex = traversalOrder.Count - 1;
                    this.Invalidate();

                    // Check if DFS is complete
                    if (currentStack.Count == 0)
                    {
                        btnNext.Enabled = false;
                        MessageBox.Show("DFS traversal complete!", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                // BFS using queue
                if (currentQueue.Count > 0 && isSimulating)
                {
                    var node = currentQueue.Dequeue();
                    traversalOrder.Add(node);
                    visitedNodes.Add(node);

                    // Add children to queue
                    foreach (var child in node.Children)
                    {
                        if (!visitedNodes.Contains(child) && !currentQueue.Contains(child))
                        {
                            currentQueue.Enqueue(child);
                        }
                    }

                    traversalIndex = traversalOrder.Count - 1;
                    this.Invalidate();

                    // Check if BFS is complete
                    if (currentQueue.Count == 0)
                    {
                        btnNext.Enabled = false;
                        MessageBox.Show("BFS traversal complete!", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void BtnReset_Click(object? sender, EventArgs e)
        {
            ResetSimulation();
        }

        private void Form1_Resize(object? sender, EventArgs e)
        {
            UpdateNodePositions();
            this.Invalidate();
        }

        private void UpdateNodePositions()
        {
            if (bfsRoot != null)
                UpdateBFSNodePositions();
            if (dfsRoot != null)
                UpdateDFSNodePositions();
        }

        private void UpdateBFSNodePositions()
        {
            if (bfsRoot == null) return;

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;
            
            // Scale factors based on window size
            float scaleX = this.ClientSize.Width / 1200f;
            float scaleY = this.ClientSize.Height / 700f;
            
            // Get all nodes in BFS tree
            var allNodes = GetAllNodesFromTree(bfsRoot);
            
            foreach (var node in allNodes)
            {
                Point originalPos = GetOriginalBFSPosition(node.Value);
                node.Position = new Point(
                    centerX + (int)((originalPos.X - 480) * scaleX),
                    centerY + (int)((originalPos.Y - 200) * scaleY) - 50
                );
            }
        }

        private void UpdateDFSNodePositions()
        {
            if (dfsRoot == null) return;

            int centerX = this.ClientSize.Width / 2;
            int centerY = this.ClientSize.Height / 2;
            
            // Scale factors based on window size
            float scaleX = this.ClientSize.Width / 1200f;
            float scaleY = this.ClientSize.Height / 700f;
            
            // Get all nodes in DFS tree
            var allNodes = GetAllNodesFromTree(dfsRoot);
            
            foreach (var node in allNodes)
            {
                Point originalPos = GetOriginalDFSPosition(node.Value);
                node.Position = new Point(
                    centerX + (int)((originalPos.X - 480) * scaleX),
                    centerY + (int)((originalPos.Y - 200) * scaleY) - 50
                );
            }
        }

        private List<TreeNode> GetAllNodesFromTree(TreeNode root)
        {
            var nodes = new List<TreeNode>();
            var queue = new Queue<TreeNode>();
            queue.Enqueue(root);
            
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                nodes.Add(node);
                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
            
            return nodes;
        }

        private Point GetOriginalBFSPosition(int value)
        {
            return value switch
            {
                1 => new Point(480, 50),
                2 => new Point(280, 150),
                3 => new Point(480, 150),
                4 => new Point(680, 150),
                5 => new Point(230, 250),
                6 => new Point(330, 250),
                7 => new Point(480, 250),
                8 => new Point(630, 250),
                9 => new Point(730, 250),
                10 => new Point(210, 350),
                11 => new Point(350, 350),
                12 => new Point(630, 350),
                13 => new Point(710, 350),
                14 => new Point(750, 350),
                _ => new Point(480, 50)
            };
        }

        private Point GetOriginalDFSPosition(int value)
        {
            return value switch
            {
                1 => new Point(480, 50),
                2 => new Point(280, 150),
                6 => new Point(480, 150),
                8 => new Point(680, 150),
                3 => new Point(230, 250),
                4 => new Point(330, 250),
                5 => new Point(230, 350),
                7 => new Point(480, 250),
                9 => new Point(680, 250),
                _ => new Point(480, 50)
            };
        }
    }
}
