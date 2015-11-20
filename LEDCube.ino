#define SR_DATA  2
#define SR_LATCH 3
#define SR_CLOCK 4
#define LEVEL_1  5
#define LEVEL_2  6
#define LEVEL_3 10
#define LEVEL_4 11

#define T_LEVEL  2
#define T_FRAME (T_LEVEL * 4)

unsigned int framebuf[4]; // Bufor ramki (niski poziom)

void setup() {
  Serial.begin(9600);
  pinMode(SR_DATA, OUTPUT);
  pinMode(SR_LATCH, OUTPUT);
  pinMode(SR_CLOCK, OUTPUT);
  pinMode(LEVEL_1, OUTPUT);
  pinMode(LEVEL_2, OUTPUT);
  pinMode(LEVEL_3, OUTPUT);
  pinMode(LEVEL_4, OUTPUT);
}

// Funkcja pomocnicza zamieniająca nimery pinów na numery warstw
int pin2lvl(int level) {
  switch(level) {
    case LEVEL_1: return 0;
    case LEVEL_2: return 1;
    case LEVEL_3: return 2;
    case LEVEL_4: return 3;
  }
}

// x - szerokość
// y - długość
// z - wysokość
// Ustawianie piksela
bool set_pixel(int x, int y, int z) {
  if(x > 3) return false;
  if(y > 3) return false;
  if(z > 3) return false;
  
  framebuf[z] |= (1 << (y * 4 + x));
  return true;
}

// Gaszenie piksela
bool clear_pixel(int x, int y, int z) {
  if(x > 3) return false;
  if(y > 3) return false;
  if(z > 3) return false;
  
  framebuf[z] ^= (1 << (y * 4 + x));
  return true;
}

// Czyszczenie bufora ramki
void clear_frame() {
  framebuf[0] = framebuf[1] = framebuf[2] = framebuf[3] = 0;
}

// Procedura wyświetlenia warstwy
void layer_refresh(int level, int ms) {
  digitalWrite(SR_LATCH, LOW);
  shiftOut(SR_DATA, SR_CLOCK, LSBFIRST, framebuf[pin2lvl(level)] & 0xFF);
  shiftOut(SR_DATA, SR_CLOCK, LSBFIRST, framebuf[pin2lvl(level)] / 0x100);
  digitalWrite(SR_LATCH, HIGH);
  
  digitalWrite(level, HIGH);
  delay(ms / 2);
  digitalWrite(level, LOW);
  delay(ms / 2);
  
  digitalWrite(SR_LATCH, LOW);
  shiftOut(SR_DATA, SR_CLOCK, MSBFIRST, 0);
  shiftOut(SR_DATA, SR_CLOCK, MSBFIRST, 0);
  digitalWrite(SR_LATCH, HIGH);
}

// Procedura wyświetlenia ramki przez określoną ilość czasu, wraz z przemiataniem warstw
void frame(int ms) {
  ms *= 2;
  for(int i = 0; i < ms; i += T_FRAME) {
    layer_refresh(LEVEL_1, T_LEVEL);
    layer_refresh(LEVEL_2, T_LEVEL);
    layer_refresh(LEVEL_3, T_LEVEL);
    layer_refresh(LEVEL_4, T_LEVEL);
  }
}

// FORMAT RAMKI: { czas ramki w ms [0-255] (wielokrotność T_FRAME), ilość zapalonych pikseli [0-255], piksele . . . }
// FORMAT PIKSELA: 0xyz, x, y, z = [0-3]

unsigned char _f1[] = {
  50, 16,
  0010, 0110, 0210, 0310,
  0011, 0111, 0211, 0311,
  0012, 0112, 0212, 0312,
  0013, 0113, 0213, 0313
};
unsigned char _f2[] = {
  50, 16,
  0030, 0120, 0210, 0300,
  0031, 0121, 0211, 0301,
  0032, 0122, 0212, 0302,
  0033, 0123, 0213, 0303
};
unsigned char _f3[] = {
  50, 16,
  0100, 0110, 0120, 0130,
  0101, 0111, 0121, 0131,
  0102, 0112, 0122, 0132,
  0103, 0113, 0123, 0133
};
unsigned char _f4[] = {
  50, 16,
  0000, 0110, 0220, 0330,
  0001, 0111, 0221, 0331,
  0002, 0112, 0222, 0332,
  0003, 0113, 0223, 0333
};
unsigned char* ANI[] = { _f1, _f2, _f3, _f4 };
unsigned char ANI_L = sizeof(ANI) / sizeof(unsigned char*);

void loop() {
  static int i = 0;
  
  // USTAWIENIE PIKSELI
  for(int j = 2; j < ANI[i][1] + 2; j++)
    set_pixel(
              ANI[i][j] / 64,       // pierwsze 3 bity - współrzędna x
              (ANI[i][j] / 8) % 8,  // kolejne 3 bity - współrzędna y
              ANI[i][j] % 8         // ostatnie 3 bity - współrzędna z
             );

  // WYŚWIETLENIE UKŁADU
  Serial.print(framebuf[0]);
  Serial.print(" ");
  Serial.print(framebuf[1]);
  Serial.print(" ");
  Serial.print(framebuf[2]);
  Serial.print(" ");
  Serial.println(framebuf[3]);
  frame(ANI[i][0]);

  // WYCZYSZCZENIE PIKSELI
  clear_frame();
  delay(1);
  
  i++;
  if(i == ANI_L) i = 0;
}
