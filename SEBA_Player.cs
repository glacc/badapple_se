
static char[] LCDMem = new char[160*160+160];

static char[] GrayTable = {(char)57600, (char)57746, (char)57892, (char)58038};

static string[] FrameData;

static char[] CharData;

static int FrameNum_Data = 0;

static int FrameNum = 0;

//static int Timer = 0;

static int BankNum = 0;

void Reset()
{
	Runtime.UpdateFrequency = 0;
	
	LCDMem = new char[160*160+160];
	
	FrameNum_Data = 0;

	FrameNum = 0;

	//Timer = 0;

	BankNum = 0;
	
	int i = 0;
	while (i < LCDMem.Length)
		LCDMem[i++] = (char)0xe100;
	
	i = 0;
	while (i < 160)
	{
		LCDMem[160*i + (i > 0 ? (i - 1) : 0)] = (char)10;
		i ++ ;
	}
	
	GetDataModule("Data_0");
	
	WriteLCD("InfoLCD", "", 2f);
	
	string LCDString = new String(LCDMem);
	WriteLCD("Screen", LCDString, 0.11f);
}

public Program()
{
	Reset();
}

void WriteLCD(string LCDName, string Text, float Size)
{
    var LCD = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextPanel;
    if (LCD != null)
	{
        LCD.ContentType = ContentType.TEXT_AND_IMAGE;
        LCD.FontSize = Size;
        LCD.WriteText(Text);
    } else
		Echo($"WriteLCD: {LCDName} is not exist.\n");
}

IMyTextPanel DataModuleOld;

bool GetDataModule(string Name)
{
	var DataModule = GridTerminalSystem.GetBlockWithName(Name) as IMyTextPanel;
	if (DataModule != null)
	{
		if (DataModuleOld != null)
			DataModuleOld.FontColor = Color.White;
		
		DataModule.FontColor = Color.Red;
		FrameData = DataModule.CustomData.Split((char)10);
		FrameNum_Data = 0;
		DataModuleOld = DataModule;
		return true;
	}
	return false;
}
		
public void Main(string arg)
{
	if (arg != "reset")
	{
		Runtime.UpdateFrequency = UpdateFrequency.Update1;
	}
	else
		Reset();
	
	//Timer ++ ;
	
	//if (Timer == 2)
	//{
	if (FrameNum_Data < FrameData.Length)
	{
		if (FrameData[FrameNum_Data] != "")
		{
			CharData = FrameData[FrameNum_Data].ToCharArray();
			FrameNum_Data ++ ;
			FrameNum ++ ;
		}
		else
		{
			BankNum ++ ;
			if (!GetDataModule("Data_" + BankNum))
			{
				Runtime.UpdateFrequency = 0;
				return;
			}
		}
	}
	
	int DataPtr = 0;
	int MemPtr = 3220;
	int PosX = 0;
	
	if (FrameNum % 2 == 0)
		MemPtr += 161;
	
	while (DataPtr < CharData.Length)
	{
		char[] CurPixs = new char[4];

		string PixHexString = CharData[DataPtr].ToString() + CharData[DataPtr + 1].ToString();
		byte PixByte = byte.Parse(PixHexString, System.Globalization.NumberStyles.HexNumber);

		string CountHexString = CharData[DataPtr + 2].ToString() + CharData[DataPtr + 3].ToString();
		byte CountByte = byte.Parse(CountHexString, System.Globalization.NumberStyles.HexNumber);
		
		byte Gray1 = (byte)((PixByte & 0xC0) >> 6);
		byte Gray2 = (byte)((PixByte & 0x30) >> 4);
		byte Gray3 = (byte)((PixByte & 0x0C) >> 2);
		byte Gray4 = (byte)(PixByte & 0x03);
		
		char Pix1 = GrayTable[Gray1];
		char Pix2 = GrayTable[Gray2];
		char Pix3 = GrayTable[Gray3];
		char Pix4 = GrayTable[Gray4];
		
		int Count = 0;
		while (Count < CountByte + 1)
		{
			LCDMem[MemPtr] = Pix1;
			LCDMem[MemPtr + 1] = Pix2;
			LCDMem[MemPtr + 2] = Pix3;
			LCDMem[MemPtr + 3] = Pix4;
			
			MemPtr += 4;
			Count ++ ;
			PosX ++ ;
			
			if (PosX >= 40)
			{
				MemPtr += 162;
				PosX = 0;
			}
		}
		
		DataPtr += 4 ;
	}
	
	var InfoStr = $"Info:\nBank: {BankNum}\nFrame: {FrameNum}\nFrame Size: {CharData.Length}\n*MemPtr: {MemPtr}\n\nGlacc@bilibili";
	WriteLCD("InfoLCD", InfoStr, 2f);
	Echo(InfoStr);
	
	if (FrameNum % 2 == 0)
	{
		string LCDString = new String(LCDMem);
		WriteLCD("Screen", LCDString, 0.11f);
	}
	
	//Timer = 0;
	//}
}
	
	