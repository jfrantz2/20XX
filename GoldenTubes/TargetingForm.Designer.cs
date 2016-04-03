namespace GoldenTubes
{
    partial class TargetingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TargetingForm));
            this.TargetButton = new System.Windows.Forms.Button();
            this.TargetName = new System.Windows.Forms.TextBox();
            this.EstimateTime = new System.Windows.Forms.Label();
            this.CurrentTime = new System.Windows.Forms.Label();
            this.UserName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TargetButton
            // 
            this.TargetButton.Location = new System.Drawing.Point(11, 62);
            this.TargetButton.Name = "TargetButton";
            this.TargetButton.Size = new System.Drawing.Size(75, 23);
            this.TargetButton.TabIndex = 0;
            this.TargetButton.Text = "Get Target";
            this.TargetButton.UseVisualStyleBackColor = true;
            this.TargetButton.Click += new System.EventHandler(this.TargetButton_Click);
            // 
            // TargetName
            // 
            this.TargetName.Location = new System.Drawing.Point(92, 65);
            this.TargetName.Name = "TargetName";
            this.TargetName.Size = new System.Drawing.Size(179, 20);
            this.TargetName.TabIndex = 1;
            // 
            // EstimateTime
            // 
            this.EstimateTime.AutoSize = true;
            this.EstimateTime.Location = new System.Drawing.Point(51, 36);
            this.EstimateTime.Name = "EstimateTime";
            this.EstimateTime.Size = new System.Drawing.Size(49, 13);
            this.EstimateTime.TabIndex = 2;
            this.EstimateTime.Text = "00:00:00";
            // 
            // CurrentTime
            // 
            this.CurrentTime.AutoSize = true;
            this.CurrentTime.Location = new System.Drawing.Point(176, 36);
            this.CurrentTime.Name = "CurrentTime";
            this.CurrentTime.Size = new System.Drawing.Size(49, 13);
            this.CurrentTime.TabIndex = 3;
            this.CurrentTime.Text = "00:00:00";
            // 
            // UserName
            // 
            this.UserName.Location = new System.Drawing.Point(107, 10);
            this.UserName.Name = "UserName";
            this.UserName.Size = new System.Drawing.Size(165, 20);
            this.UserName.TabIndex = 4;
            this.UserName.TextChanged += new System.EventHandler(this.UserName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Your Main Nation";
            // 
            // TargetingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 100);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.UserName);
            this.Controls.Add(this.CurrentTime);
            this.Controls.Add(this.EstimateTime);
            this.Controls.Add(this.TargetName);
            this.Controls.Add(this.TargetButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TargetingForm";
            this.Text = "TargetingForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.TargetingForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button TargetButton;
        private System.Windows.Forms.TextBox TargetName;
        private System.Windows.Forms.Label EstimateTime;
        private System.Windows.Forms.Label CurrentTime;
        private System.Windows.Forms.TextBox UserName;
        private System.Windows.Forms.Label label1;
    }
}