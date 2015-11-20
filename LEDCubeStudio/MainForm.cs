using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LEDCubeStudio {
  public partial class MainForm : Form {
    public static List<Frame> frames = new List<Frame>();
    private byte frame;

    public MainForm() {
      LedImage.OpacityCache = true;

      InitializeComponent();

      frames.Add(new Frame());
      frames[0].Duration = 48;
      changeFrame(0);
      foreach(LedImage led in container.Controls) led.CoordinationsLabel = coords;
    }

    private void changeFrame(byte fnum) {
      if(fnum > frames.Count) throw new ArgumentOutOfRangeException("Próba przejścia do ramki, która nie istnieje.");
      frame = fnum;

      foreach(LedImage led in container.Controls) {
        led.LedList = null;
        led.Lit = false;
      }
      foreach(String ledname in frames[frame].LEDs) ((LedImage)container.Controls.Find(ledname, false)[0]).Lit = true;
      foreach(LedImage led in container.Controls) led.LedList = frames[frame].LEDs;

      duration.Text = frames[frame].Duration.ToString();
      frameNumber.Text = (frame + 1).ToString() + " z " + frames.Count.ToString();

      if(frame == 0) prevFrame.Enabled = false;
      else prevFrame.Enabled = true;

      if(frame == frames.Count - 1) nextFrame.Enabled = false;
      else nextFrame.Enabled = true;
    }

    private void addFrame_Click(object sender, EventArgs e) {
      frames.Insert(frame + 1, new Frame());
      changeFrame((byte)(frame + 1));
    }

    private void deleteFrame_Click(object sender, EventArgs e) {
      frames.RemoveAt(frame);
      if(frames.Count == 0) {
        frames.Add(new Frame());
        frames[0].Duration = 48;
        changeFrame(0);
      } else if(frames.Count == frame) {
        changeFrame((byte)(frame - 1));
      } else {
        changeFrame(frame);
      }
    }

    private void prevFrame_Click(object sender, EventArgs e) {
      changeFrame((byte)(frame - 1));
    }

    private void nextFrame_Click(object sender, EventArgs e) {
      changeFrame((byte)(frame + 1));
    }

    private void duration_TextChanged(object sender, EventArgs e) {
      try {
        frames[frame].Duration = Byte.Parse(duration.Text);
        if(frames[frame].Duration % 8 == 0) duration.BackColor = Color.Lime;
        else duration.BackColor = Color.White;
        if(frames[frame].Duration > 255) duration.BackColor = Color.Red;
      } catch(FormatException) {
        duration.BackColor = Color.Red;
      } catch(OverflowException) {
        duration.BackColor = Color.DarkRed;
      }
    }

    private void play_Click(object sender, EventArgs e) {
      timer.Enabled = !timer.Enabled;
      if(play.Text == "▶") {
        play.Text = "❚❚";
        play.ToolTipText = "Wstrzymaj animację";
      } else {
        play.Text = "▶";
        play.ToolTipText = "Odtwórz animację";
      }
    }

    private int counter = 0;
    private void timer_Tick(object sender, EventArgs e) {
      counter += 8;
      if(counter > frames[frame].Duration) {
        counter = 0;
        if(frame + 1 == frames.Count) changeFrame(0);
        else changeFrame((byte)(frame + 1));
      }
    }

    private void saveAnim_Click(object sender, EventArgs e) {
      String ani = "unsigned char *ANI[] = { ", frames = "";
      int i = 0;

      foreach(Frame frame in MainForm.frames) {
        ani += "_f" + i.ToString();
        if(i != MainForm.frames.Count - 1) ani += ", ";

        frames += "unsigned char _f" + i.ToString() + "[] = { " + frame.Duration.ToString() + ", " + frame.LEDs.Count.ToString();
        foreach(String led in frame.LEDs) frames += ", " + led.Replace("led", "0");
        frames += " };\r\n";

        i++;
      }

      ani += " };\r\nunsigned char ANI_L = " + MainForm.frames.Count.ToString() + ";\r\n";

      if(saveDialog.ShowDialog() == DialogResult.OK) {
        System.IO.StreamWriter file = new System.IO.StreamWriter(saveDialog.FileName);
        file.Write(frames + ani);
        file.Close();
      }
    }

    private void openAnim_Click(object sender, EventArgs e) {
      if(openDialog.ShowDialog() == DialogResult.OK) {
        frames.Clear();

        String line = "";
        System.IO.StreamReader file = new System.IO.StreamReader(openDialog.FileName);
        while((line = file.ReadLine()) != null) {
          line = line.Replace("unsigned char ", "");
          line = line.Replace(" =", "");
          line = line.Replace("[]", "");
          line = line.Replace("{", "");
          line = line.Replace("}", "");
          line = line.Replace(";", "");
          line = line.Replace(",", "");
          line = line.Replace("  ", " ");

          if(line.StartsWith("_f")) {
            int i = 0, max = 0;
            Frame f = new Frame();
            foreach(String val in line.Split(' ')) {
              if(i != 0) {
                if(i == 1) f.Duration = Byte.Parse(val);
                else if(i == 2) max = Byte.Parse(val);
                else if(i < (max + 3)) f.LEDs.Add("led" + val.Substring(1));
              }
              i++;
            }
            frames.Add(f);
          } else continue;
        }
        file.Close();

        changeFrame(0);
      }
    }

    private void about_Click(object sender, EventArgs e) {
      Version version = Assembly.GetExecutingAssembly().GetName().Version;
      MessageBox.Show(this,
        String.Format(
          "Matriksoft LED Cube Studio\r\nwersja {0}.{1}.{2}\r\n\r\n© 2015 Mateusz Karcz. Wszelkie prawa zastrzeżone.",
          version.Major, version.Minor, version.Build
        ),
        "O programie LED Cube Studio"
      );
    }
  }
}
