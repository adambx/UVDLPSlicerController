﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
namespace UV_DLP_3D_Printer
{
    /*
     * This class holds some information about the 
     * slicing and building parameters
     */
    public class SliceBuildConfig
    {
        public enum eBuildDirection 
        {
            Top_Down,
            Bottom_Up
        }
        public double dpmmX; // dots per mm x
        public double dpmmY; // dots per mm y
        public int xres, yres; // the resolution of the output image in pixels
        public double ZThick; // thickness of the z layer - slicing height
        public int layertime_ms; // time to project image per layer in milliseconds
        public int firstlayertime_ms; // first layer exposure time 
        public int blanktime_ms; // blanking time between layers
        public int plat_temp; // desired platform temperature in celsius 
        public bool exportgcode; // export the gcode file when slicing
        public bool exportsvg; // export the svg slices when building
        public bool exportimages; // export image slices when building
        public eBuildDirection direction;
        public double liftdistance; // distance to lift and retract

        private String m_headercode; // inserted at beginning of file
        private String m_footercode; // inserted at end of file
        private String m_preliftcode; // inserted before each slice
        private String m_postliftcode; // inserted after each slice
        public int XOffset, YOffset; // the X/Y pixel offset used 

        private String[] m_defheader = 
        {
            "(********** Header Start ********)\r\n", // 
            "(Generated by UV - DLP Slicer)\r\n",
            "G21 (Set units to be mm)\r\n", 
            "G91 (Relative Positioning)\r\n",
            "M17 (Enable motors)\r\n",
            "(********** Header End **********)\r\n", // 
            //"()\r\n"
        };
        private String[] m_deffooter = 
        {
            "(********** Footer Start ********)\r\n", // 
            "\r\n",
            "(<Completed>)\r\n", // a marker for completed            
            "(********** Footer End ********)\r\n", // 
        };

        private String[] m_defpreslice = 
        {
            "\r\n"
        };

        private String[] m_defpostslice = 
        {
            "\r\n"
        };
        private void SetDefaultCodes()
        {
            StringBuilder sb = new StringBuilder();
            foreach (String s in m_defheader)
                sb.Append(s);
            HeaderCode = sb.ToString();

            sb = new StringBuilder(); // clear
            foreach (String s in m_deffooter)
                sb.Append(s);
            FooterCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defpreslice)
                sb.Append(s);
            PreLiftCode = sb.ToString();

            sb = new StringBuilder();
            foreach (String s in m_defpreslice)
                sb.Append(s);
            PostLiftCode = sb.ToString();
            
        }
        public String HeaderCode
        {
            get { return m_headercode; }
            set { m_headercode = value; }
        }
        public String FooterCode
        {
            get { return m_footercode; }
            set { m_footercode = value; }
        }
        public String PreLiftCode
        {
            get { return m_preliftcode; }
            set { m_preliftcode = value; }
        }

        public String PostLiftCode
        {
            get { return m_postliftcode; }
            set { m_postliftcode = value; }
        }

        /*
         Copy constructor
         */
        public SliceBuildConfig(SliceBuildConfig source) 
        {
            dpmmX = source.dpmmX; // dots per mm x
            dpmmY = source.dpmmY; // dots per mm y
            xres = source.xres;
            yres = source.yres; // the resolution of the output image
            ZThick = source.ZThick; // thickness of the z layer - slicing height
            layertime_ms = source.layertime_ms; // time to project image per layer in milliseconds
            firstlayertime_ms = source.firstlayertime_ms;
            blanktime_ms = source.blanktime_ms;
            plat_temp = source.plat_temp; // desired platform temperature in celsius 
            exportgcode = source.exportgcode; // export the gcode file when slicing
            exportsvg = source.exportsvg; // export the svg slices when building
            exportimages = source.exportimages; // export image slices when building
            m_headercode = source.m_headercode; // inserted at beginning of file
            m_footercode = source.m_footercode; // inserted at end of file
            m_preliftcode = source.m_preliftcode; // inserted between each slice            
            m_postliftcode = source.m_postliftcode; // inserted between each slice    
            liftdistance = source.liftdistance;
            direction = source.direction;
            XOffset = source.XOffset;
            YOffset = source.YOffset;
        }

        public SliceBuildConfig() 
        {
            layertime_ms = 5000;// 5 seconds default
            CreateDefault();
        }
        public void UpdateFrom(MachineConfig mf)
        {
            dpmmX = mf.PixPerMMX; //10 dots per mm
            dpmmY = mf.PixPerMMY;// 10;
            xres = mf.XRes;
            yres = mf.YRes;            
        }
        public void CreateDefault() 
        {
            layertime_ms = 1000;// 1 second default
            firstlayertime_ms = 5000;
            blanktime_ms = 2000; // 2 seconds blank
            xres = 1024;
            yres = 768;
            ZThick = .025;
            plat_temp = 75;
            dpmmX = 102.4;
            dpmmY = 76.8;
            XOffset = 0;
            YOffset = 0;
            exportgcode = true;
            exportsvg = false;
            exportimages = false;
            direction = eBuildDirection.Bottom_Up;
            liftdistance = 5.0;
            SetDefaultCodes(); // set up default gcodes
        }

        /*This is used to serialize to the GCode post-header info*/
        public bool Load(String filename) 
        {
            try
            {
                LoadGCodes();
                XmlReader xr = (XmlReader)XmlReader.Create(filename);
                xr.ReadStartElement("SliceBuildConfig");
                dpmmX = double.Parse(xr.ReadElementString("DotsPermmX"));
                dpmmY = double.Parse(xr.ReadElementString("DotsPermmY"));
                xres = int.Parse(xr.ReadElementString("XResolution"));
                yres = int.Parse(xr.ReadElementString("YResolution"));
                ZThick = double.Parse(xr.ReadElementString("SliceHeight"));
                layertime_ms = int.Parse(xr.ReadElementString("LayerTime"));
                firstlayertime_ms = int.Parse(xr.ReadElementString("FirstLayerTime"));
                blanktime_ms = int.Parse(xr.ReadElementString("BlankTime"));
                plat_temp = int.Parse(xr.ReadElementString("PlatformTemp"));
                exportgcode = bool.Parse(xr.ReadElementString("ExportGCode"));
                exportsvg = bool.Parse(xr.ReadElementString("ExportSVG"));
                exportimages = bool.Parse(xr.ReadElementString("ExportImages")); ;
                XOffset = int.Parse(xr.ReadElementString("XOffset"));
                YOffset = int.Parse(xr.ReadElementString("YOffset"));
                direction = (eBuildDirection)Enum.Parse(typeof(eBuildDirection), xr.ReadElementString("Direction"));
                liftdistance = double.Parse(xr.ReadElementString("LiftDistance"));
                xr.ReadEndElement();
                xr.Close();
                
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }       
        }
        public bool Save(String filename)
        {
            try 
            {
                XmlWriter xw =XmlWriter.Create(filename);
                xw.WriteStartElement("SliceBuildConfig");
                xw.WriteElementString("DotsPermmX", dpmmX.ToString());
                xw.WriteElementString("DotsPermmY", dpmmY.ToString());
                xw.WriteElementString("XResolution", xres.ToString());
                xw.WriteElementString("YResolution", yres.ToString());
                xw.WriteElementString("SliceHeight", ZThick.ToString());
                xw.WriteElementString("LayerTime", layertime_ms.ToString());
                xw.WriteElementString("FirstLayerTime", firstlayertime_ms.ToString());
                xw.WriteElementString("BlankTime", blanktime_ms.ToString());                
                xw.WriteElementString("PlatformTemp", plat_temp.ToString());
                xw.WriteElementString("ExportGCode", exportgcode.ToString());
                xw.WriteElementString("ExportSVG", exportsvg.ToString());
                xw.WriteElementString("ExportImages", exportimages.ToString());
                xw.WriteElementString("XOffset", XOffset.ToString());
                xw.WriteElementString("YOffset", YOffset.ToString());
                xw.WriteElementString("Direction", direction.ToString());
                xw.WriteElementString("LiftDistance", liftdistance.ToString());

                xw.WriteEndElement();
                xw.Close();
                return true;
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }            
        }

        // these get stored to the gcode file as a reference
        public override String ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(****Build and Slicing Parameters****)\r\n");
            sb.Append("(dots per mm X           = " + dpmmX + " )\r\n");
            sb.Append("(dots per mm Y           = " + dpmmY + " )\r\n");
            sb.Append("(X resolution            = " + xres + " )\r\n");
            sb.Append("(Y resolution            = " + yres + " )\r\n");
            sb.Append("(X Pixel Offset          = " + XOffset + " )\r\n");
            sb.Append("(Y Pixel Offset          = " + YOffset + " )\r\n");
            sb.Append("(Layer thickness         = " + ZThick + " )\r\n");
            sb.Append("(Layer Time              = " + layertime_ms + ")\r\n");
            sb.Append("(First Layer Time        = " + firstlayertime_ms + ")\r\n");
            sb.Append("(Blanking Layer Time     = " + blanktime_ms + ")\r\n");
            sb.Append("(Platform Temp           = " + plat_temp + ")\r\n");
            sb.Append("(Build Direction         = " + direction.ToString() + ")\r\n");
            sb.Append("(Lift Distance           = " + liftdistance.ToString() + ")\r\n");            
            return sb.ToString();
        }

        public void LoadGCodes() 
        {
            try
            {

                String profilepath = Path.GetDirectoryName(UVDLPApp.Instance().m_appconfig.m_cursliceprofilename);
                profilepath += UVDLPApp.m_pathsep;
                profilepath += Path.GetFileNameWithoutExtension(UVDLPApp.Instance().m_appconfig.m_cursliceprofilename);
                if (!Directory.Exists(profilepath))
                {
                    Directory.CreateDirectory(profilepath);
                    SetDefaultCodes();
                    SaveDefaultGCodes();// save the default gcode files for this machine
                }
                else
                {
                    //load the files
                    m_headercode = LoadFile(profilepath + UVDLPApp.m_pathsep + "start.gcode");
                    m_footercode = LoadFile(profilepath + UVDLPApp.m_pathsep + "end.gcode");
                    m_preliftcode = LoadFile(profilepath + UVDLPApp.m_pathsep + "prelift.gcode");
                    m_postliftcode = LoadFile(profilepath + UVDLPApp.m_pathsep + "postlift.gcode");
                }
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
            }
        }

        public String LoadFile(String filename) 
        {
            try
            {
                return File.ReadAllText(filename);
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return "";
            }
        }

        public bool SaveFile(String filename, String contents) 
        {
            try
            {
                File.WriteAllText(filename, contents);
                return true;
            }
            catch (Exception ex) 
            {
                DebugLogger.Instance().LogRecord(ex.Message);
                return false;
            }
        }
        public void SaveDefaultGCodes() 
        {
            String profilepath = Path.GetDirectoryName(UVDLPApp.Instance().m_appconfig.m_cursliceprofilename);
            profilepath += UVDLPApp.m_pathsep;
            profilepath += Path.GetFileNameWithoutExtension(UVDLPApp.Instance().m_appconfig.m_cursliceprofilename);

            SaveFile(profilepath + UVDLPApp.m_pathsep + "start.gcode",m_headercode);
            SaveFile(profilepath + UVDLPApp.m_pathsep + "end.gcode", m_footercode);
            SaveFile(profilepath + UVDLPApp.m_pathsep + "prelift.gcode", m_preliftcode);
            SaveFile(profilepath + UVDLPApp.m_pathsep + "postlift.gcode", m_postliftcode);
        }
    }
}
