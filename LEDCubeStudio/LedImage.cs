using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LEDCubeStudio {
  public partial class LedImage : PictureBox {
    private Bitmap led_on, led_off, led_hov;
    private static Dictionary<byte, Bitmap> led_on_cache, led_off_cache, led_hov_cache;

    private static bool opacity_cache = false;
    public static bool OpacityCache {
      get { return opacity_cache; }
      set {
        opacity_cache = value;
        if(opacity_cache) {
          led_on_cache = new Dictionary<byte, Bitmap>();
          led_off_cache = new Dictionary<byte, Bitmap>();
          led_hov_cache = new Dictionary<byte, Bitmap>();
        }
      }
    }

    public LedImage() {

      SizeMode = PictureBoxSizeMode.Zoom;
      Width = 50;
      Height = 50;
      Opacity = 255;
      Lit = false;
    }

    private bool light = false;
    [EditorBrowsable(EditorBrowsableState.Always)]
    public bool Lit {
      get { return light; }
      set {
        light = value;

        if(value) this.Image = led_on;
        else this.Image = led_off;
      }
    }

    private byte opacity;
    [EditorBrowsable(EditorBrowsableState.Always)]
    public byte Opacity {
      get { return opacity; }
      set {
        if(value > 255) throw new ArgumentOutOfRangeException();
        if(value < 0) throw new ArgumentOutOfRangeException();
        opacity = value;

        led_on = new Bitmap(Properties.Resources.led_on);
        led_off = new Bitmap(Properties.Resources.led_off);
        led_hov = new Bitmap(Properties.Resources.led_hov);

        if(opacity < 255) {
          if(OpacityCache) {
            if(led_on_cache.ContainsKey(opacity)) {
              led_on = led_on_cache[opacity];
              led_off = led_off_cache[opacity];
              led_hov = led_hov_cache[opacity];
            } else {
              ComputeOpacity();
              led_on_cache[opacity] = new Bitmap(led_on);
              led_off_cache[opacity] = new Bitmap(led_off);
              led_hov_cache[opacity] = new Bitmap(led_hov);
            }
          } else ComputeOpacity();
        }
        Lit = Lit;
      }
    }

    private List<String> olist;
    [EditorBrowsable(EditorBrowsableState.Always)]
    public List<String> LedList {
      get { return olist; }
      set { olist = value; }
    }

    private Control coorlabel;
    [EditorBrowsable(EditorBrowsableState.Always)]
    public Control CoordinationsLabel {
      get { return coorlabel; }
      set { coorlabel = value; }
    }

    private void ComputeOpacity() {
      for(int w = 0; w < led_on.Width; w++)
        for(int h = 0; h < led_on.Height; h++) {
          Color c = led_on.GetPixel(w, h);
          led_on.SetPixel(w, h, Color.FromArgb(Convert.ToInt32(Math.Sqrt(opacity * c.A)), c));

          c = led_off.GetPixel(w, h);
          led_off.SetPixel(w, h, Color.FromArgb(Convert.ToInt32(Math.Sqrt(opacity * c.A)), c));

          c = led_hov.GetPixel(w, h);
          led_hov.SetPixel(w, h, Color.FromArgb(Convert.ToInt32(Math.Sqrt(opacity * c.A)), c));
        }
    }

    protected override void OnMouseHover(EventArgs e) {
      base.OnMouseHover(e);

      this.Image = led_hov;
      if(CoordinationsLabel != null) CoordinationsLabel.Text = this.Name;
    }

    protected override void OnMouseLeave(EventArgs e) {
      base.OnMouseLeave(e);

      Lit = Lit;
      if(CoordinationsLabel != null) CoordinationsLabel.Text = "";
    }

    protected override void OnMouseClick(MouseEventArgs e) {
      base.OnMouseClick(e);
      if(LedList != null) {
        if(Lit) LedList.Remove(this.Name);
        else LedList.Add(this.Name);
      }

      Lit = !Lit;
    }
  }
}
