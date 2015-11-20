using System;
using System.Collections.Generic;
using System.Text;

namespace LEDCubeStudio {
  public class Frame {
    private byte duration = 48;
    public byte Duration {
      get { return duration; }
      set { duration = value; }
    }

    public List<String> LEDs = new List<String>();
  }
}
