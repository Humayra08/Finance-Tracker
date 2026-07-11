namespace Finance_Tracker
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Kept intentionally empty — every control in this project is created
        // in code inside MainForm.cs (BuildHeader, BuildSummaryCards, etc.)
        // rather than through the Designer's drag-and-drop surface.
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            SuspendLayout();
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(900, 600);
            Name = "MainForm";
            ResumeLayout(false);
        }
    }
}
