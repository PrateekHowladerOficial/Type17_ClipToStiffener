using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.ModelInternal;
using Tekla.Structures.Plugins;
using Tekla.Structures.Solid;
using static Tekla.Structures.Model.Chamfer;
using Identifier = Tekla.Structures.Identifier;

namespace Type17_ClipToStiffener

{
    public class PluginData
    {
        #region Fields



        
        [StructuresField("Thickness")]
        public double Thickness;

        [StructuresField("Material")]
        public string Material;

        [StructuresField("Name")]
        public string Name;

        [StructuresField("Finish")]
        public string Finish;

        [StructuresField("Gap")]
        public double Gap;

        [StructuresField("PlateHight")]
        public double PlateHight;

        [StructuresField("TopOffset")]
        public double TopOffset;

        [StructuresField("PlateWidth1")]
        public double PlateWidth1;

        [StructuresField("PlateWidth2")]
        public double PlateWidth2;

        [StructuresField("Platepoaition")]
        public int Platepoaition;

        [StructuresField("FlagBolt")]
        public int FlagBolt;

        [StructuresField("FlagWasher1")]
        public int FlagWasher1;

        [StructuresField("FlagWasher2")]
        public int FlagWasher2;

        [StructuresField("FlagWasher3")]
        public int FlagWasher3;

        [StructuresField("FlagNut1")]
        public int FlagNut1;

        [StructuresField("FlagNut2")]
        public int FlagNut2;

        [StructuresField("BoltSize")]
        public int BoltSize;

        [StructuresField("BoltStandard")]
        public int BoltStandard;

        [StructuresField("BoltToletance")]
        public double BoltToletance;

        [StructuresField("BoltThreadMat")]
        public int BoltThreadMat;

        [StructuresField("BA1yCount")]
        public int BA1yCount;

        [StructuresField("BA1yText")]
        public string BA1yText;

        [StructuresField("BA1xCount")]
        public int BA1xCount;

        [StructuresField("BA1xText")]
        public string BA1xText;

        [StructuresField("BA2yCount")]
        public int BA2yCount;

        [StructuresField("BA2yText")]
        public string BA2yText;

        [StructuresField("BA2xCount")]
        public int BA2xCount;

        [StructuresField("BA2xText")]
        public string BA2xText;

        [StructuresField("BA1OffsetX")]
        public double BA1OffsetX;

        [StructuresField("BA1OffsetY")]
        public double BA1OffsetY;

        [StructuresField("BA2OffsetX")]
        public double BA2OffsetX;

        [StructuresField("BA2OffsetY")]
        public double BA2OffsetY;

        #endregion
    }

    [Plugin("Type17_ClipToStiffener")]
    [PluginUserInterface("Type17_ClipToStiffener.MainForm")]
    public class Type17_ClipToStiffener : PluginBase
    {
        Model myModel = new Model();
        #region Fields
        private Model _Model;
        private PluginData _Data;

        private double _Thickness;
        private string _Material = string.Empty;
        private string _Name = string.Empty;
        private string _Finish = string.Empty;
        private double _Gap ;
        private double _PlateHight;
        private double _TopOffset ;
        private double _PlateWidth1;
        private double _PlateWidth2;
        private int _Platepoaition;
        private int _FlagBolt;
        private int _FlagWasher1;
        private int _FlagWasher2;
        private int _FlagWasher3;
        private int _FlagNut1;
        private int _FlagNut2;
        private int _BoltSize;
        private int _BoltStandard;
        private double _BoltToletance;
        private int _BoltThreadMat;
        private int _BA1yCount;
        private string _BA1yText;
        private int _BA1xCount;
        private string _BA1xText;
        private int _BA2yCount;
        private string _BA2yText;
        private int _BA2xCount;
        private string _BA2xText;
        private double _BA1OffsetX;
        private double _BA1OffsetY;
        private double _BA2OffsetX;
        private double _BA2OffsetY;

        private List<string> _BoltStandardEnum = new List<string>
        {
            "8.8XOX",
            "4.6CSK",
            "4.6CUP",
            "4.6FIRE",
            "4.6XOX",
            "8.8CSK",
            "8.8CUP",
            "8.8FIRE",
            "8.8XOX",
            "E.B",
            "HSFG-XOX",
            "UNDEFINED_BOLT",
            "UNDEFINED_STUD"
            };

        private List<double> _BoltSizeEnum = new List<double>
        {
            10.00,
            12.00,
            16.00,
            20.00,
            24.00,
            30.00
        };
       

        #endregion

        #region Properties
        private Model Model
        {
            get { return this._Model; }
            set { this._Model = value; }
        }

        private PluginData Data
        {
            get { return this._Data; }
            set { this._Data = value; }
        }
        #endregion

        #region Constructor
        public Type17_ClipToStiffener(PluginData data)
        {
            Model = new Model();
            Data = data;
        }
        #endregion

        #region Overrides
        public override List<InputDefinition> DefineInput()
        {
            //
            // This is an example for selecting two points; change this to suit your needs.
            //
            List<InputDefinition> PointList = new List<InputDefinition>();
            Picker Picker = new Picker();
            var part = Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Primary object");
            var partno1 = part;
            PointList.Add(new InputDefinition(partno1.Identifier));
            part = Picker.PickObject(Picker.PickObjectEnum.PICK_ONE_OBJECT, "Pick Secondary object");
            var partno2 = part;
            PointList.Add(new InputDefinition(partno2.Identifier));
            return PointList;
        }

        public override bool Run(List<InputDefinition> Input)
        {
            try
            {
                GetValuesFromDialog();
                var beam1Input = Input[0];
                Beam beam1 = myModel.SelectModelObject(beam1Input.GetInput() as Identifier) as Beam;                
                Beam beam2 = myModel.SelectModelObject(Input[1].GetInput() as Identifier) as Beam;

                ContourPlate cp = ContourFitPart(beam1, beam2, _Gap, _Material);
                bool flag = true;
                if(_Platepoaition == 0)
                    flag = true;
                else
                    flag = false;
                ArrayList poliplates = polibeamPlate(beam1,beam2,cp,_Gap,_PlateWidth1,_PlateWidth2,_PlateHight,_Thickness,_TopOffset,_Material,flag);
                myModel.CommitChanges();
                BoltArray(cp,beam2, poliplates[0] as Part, poliplates[1] as Part);
            }
            catch (Exception Exc)
            {
                MessageBox.Show(Exc.ToString());
            }

            return true;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the values from the dialog and sets the default values if needed
        /// </summary>
        private void GetValuesFromDialog()
        {
            _Thickness = Data.Thickness;
            _Material = Data.Material;
            _Name = Data.Name;
            _Finish = Data.Finish;
            _Gap = Data.Gap;
            _PlateHight = Data.PlateHight;
            _PlateWidth1 = Data.PlateWidth1;
            _PlateWidth2 = Data.PlateWidth2;
            _TopOffset = Data.TopOffset;
            _Platepoaition = Data.Platepoaition;
            _FlagBolt = Data.FlagBolt;
            _FlagWasher1 = Data.FlagWasher1;
            _FlagWasher2 = Data.FlagWasher2;
            _FlagWasher3 = Data.FlagWasher3;
            _FlagNut1 = Data.FlagNut1;
            _FlagNut2 = Data.FlagNut2;
            _BoltSize = Data.BoltSize;
            _BoltStandard = Data.BoltStandard;
            _BoltToletance = Data.BoltToletance;
            _BoltThreadMat = Data.BoltThreadMat;
            _BA1yCount = Data.BA1yCount;
            _BA1yText = Data.BA1yText;
            _BA1xCount = Data.BA1xCount;
            _BA1xText = Data.BA1xText;
            _BA2yCount = Data.BA2yCount;
            _BA2yText = Data.BA2yText;
            _BA2xCount = Data.BA2xCount;
            _BA2xText = Data.BA2xText;
            _BA1OffsetX = Data.BA1OffsetX;
            _BA1OffsetY = Data.BA1OffsetY;
            _BA2OffsetX = Data.BA2OffsetX;
            _BA2OffsetY = Data.BA2OffsetY;
            if (IsDefaultValue(_Thickness))
            {
                _Thickness = 10;
            }
            if (IsDefaultValue(_Material))
            {
                _Material = "IS2062";
            }
            if (IsDefaultValue(_Name))
            {
                _Name = "Clip to Stiffener";
            }
            if (IsDefaultValue(_Finish))
            {
                _Finish = "foo";

            }
            if (IsDefaultValue(_Gap))
            {
                _Gap = 20;
            }
            if (IsDefaultValue(_PlateHight))
            {
                _PlateHight = 200;
            }
            if (IsDefaultValue(_PlateWidth1))
            {
                _PlateWidth1 = 75;
            }
            if (IsDefaultValue(_PlateWidth2))
            {
                _PlateWidth2 = 100;
            }
            if (IsDefaultValue(_TopOffset))
            {
                _TopOffset = 0;
            }
            if (IsDefaultValue(_Platepoaition))
            {
                _Platepoaition = 0;
            }
            if(IsDefaultValue(_BoltSize))
            {
                _BoltSize = 0;
            }
            if(IsDefaultValue(_BoltStandard))
            {
                _BoltStandard = 0;
            }
            if(IsDefaultValue(_BoltThreadMat))
            {
                _BoltThreadMat = 0;
            }
            if(IsDefaultValue(_BoltToletance))
            {
                _BoltToletance = 3;
            }
            if(IsDefaultValue(_FlagBolt))
            {
                _FlagBolt = 0;
            }
            if(IsDefaultValue(_FlagWasher1))
            {
                _FlagWasher1 = 0;
            }
            if(IsDefaultValue(_FlagWasher2))
            {
                _FlagWasher2 = 1;
            }
            if(IsDefaultValue(_FlagWasher3))
            {
                _FlagWasher3 = 1;
            }
            if(IsDefaultValue(_FlagNut1))
            {
                _FlagNut1 = 0;
            }
            if(IsDefaultValue(_FlagNut2))
            {
                _FlagNut2 = 1;
            }
            if(IsDefaultValue(_BA1xCount))
            {
                _BA1xCount = 3;
            }
            if (IsDefaultValue(_BA1xText))
            {
                _BA1xText = "50";
            }
            if(IsDefaultValue(_BA1yCount))
            {
                _BA1yCount = 1;
            }
            if(IsDefaultValue(_BA1yText))
            {
                _BA1yText = "";
            }
            if(IsDefaultValue(_BA2xCount))
            {
                _BA2xCount = 3;
            }
            if(IsDefaultValue(_BA2xText))
            {
                _BA2xText = "50";
            }
            if(IsDefaultValue(_BA2yCount))
            {
                _BA2yCount = 1;
            }
            if(IsDefaultValue(_BA2yText))
            {
                _BA2yText = "";
            }
            if(IsDefaultValue(_BA1OffsetX))
                { _BA1OffsetX = 0; }
            if(IsDefaultValue(_BA1OffsetY))
                { _BA1OffsetY = 0; }
            if (IsDefaultValue(_BA2OffsetX))
                { _BA2OffsetX = 0; }
            if( IsDefaultValue(_BA2OffsetY))
            {
                _BA2OffsetY = 0;
            }
            
        }
        
        private ContourPlate ContourFitPart(Beam beam1, Beam beam2 , double gap, string material)
        {
            
            List<Face_> beam1_faces = get_faces(beam1);
            ArrayList beam1_centerLine = beam1.GetCenterLine(false);
            ArrayList beam2_centerLine = beam2.GetCenterLine(false);
            Face_ face = null;
            int beam2FaceIndex = -1, beam1FaceIndex = -1;
            Point mid = MidPoint(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point);
            if (Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[0].Face)) < Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[10].Face)))
            {
                face = beam1_faces[0];
                beam1FaceIndex = 2;
            }
            else
            {
                face = beam1_faces[10];
                beam1FaceIndex = 8;
            }
            Fitting fitting = new Fitting();
            fitting.Plane = new Plane();
            Vector vector = face.Vector;
            Point point = Get_Points(face.Face)[0] as Point;
         
            Point point1 = new Point(point.X + gap * vector.X, point.Y + gap * vector.Y, point.Z + gap * vector.Z);
            fitting.Plane.Origin = point1;
            GetFaceAxes(face.Face, out Vector xAxis, out Vector yAxis);
            fitting.Plane.AxisX = xAxis;
            fitting.Plane.AxisY = yAxis;
            fitting.Father = beam2;
            fitting.Insert();
            List<Face_> beam2_faces = get_faces(beam2);

            if (Distance.PointToPlane(MidPoint(beam1_centerLine[0] as Point, beam1_centerLine[1] as Point), ConvertFaceToGeometricPlane(beam2_faces[12].Face)) < Distance.PointToPlane(MidPoint(beam1_centerLine[0] as Point, beam1_centerLine[1] as Point), ConvertFaceToGeometricPlane(beam2_faces[13].Face)))
            {
                beam2FaceIndex = 12;
            }
            else
            {
                beam2FaceIndex = 13;
            }

            ArrayList points = Get_Points(beam2_faces[beam2FaceIndex].Face);
            List<Point> points1 = new List<Point>();
            GeometricPlane plane = ConvertFaceToGeometricPlane(beam2_faces[beam2FaceIndex].Face),plane1 = ConvertFaceToGeometricPlane(beam2_faces[5].Face);
            foreach (Point p in points)
            {
                
                   if(IsPointOnPlane(p,plane1))
                    points1.Add(p);
               
            }

            Point p1 = Intersection.LineToPlane(new Line(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point), plane);
            Line line = Intersection.PlaneToPlane(plane, ConvertFaceToGeometricPlane(beam2_faces[5].Face));
             
            Point closest = null, farest = null;
            bool partCutFlag = true;
            if (Distance.PointToPoint(points1[0], mid) < Distance.PointToPoint(points1[1], mid))
            {
                farest = points1[1];
                closest = points1[0];
            }
            else
            {
                if (Distance.PointToPoint(points1[0], mid) != Distance.PointToPoint(points1[1], mid))
                {
                    closest = points1[1];
                    farest = points1[0];
                }
                else
                    partCutFlag = false;
            }
            ArrayList countourPoints = new ArrayList();
            if(partCutFlag)
            {
                Point center = Intersection.LineToPlane(new Line(MidPoint(closest, farest), FindClosestPointOnPlane(plane1, mid)), plane);
                Point point2 = new Point();
                GeometricPlane geometricPlane = ConvertFaceToGeometricPlane(beam2_faces[4].Face);
                if (IsPointOnPlane(closest, geometricPlane))
                {
                    point2 = FindClosestPointOnPlane(ConvertFaceToGeometricPlane(beam2_faces[6].Face), closest);
                }
                else
                    point2 = FindClosestPointOnPlane(geometricPlane, closest);
                double distance = CalculateDistanceBetweenFaces(beam2_faces[5].Face, beam2_faces[11].Face);


                GeometricPlane geometricPlane2 = ConvertFaceToGeometricPlane(beam1_faces[5].Face);


                foreach (Point po in new List<Point> { farest, point2, center })
                {
                    Point point3 = FindClosestPointOnPlane(geometricPlane2, po);
                    ContourPoint contourPoint = new ContourPoint(point3, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                if (countourPoints != null && partCutFlag)
                {
                    ContourPlate cp = new ContourPlate();
                    cp.Contour.ContourPoints = countourPoints;

                    cp.Profile.ProfileString = "PLT" + (distance * 2.5).ToString();
                    cp.Class = BooleanPart.BooleanOperativeClassName;
                    cp.Material.MaterialString = material;
                    cp.Position.Depth = Position.DepthEnum.MIDDLE;
                    cp.Insert();
                    BooleanPart boolpart1 = new BooleanPart();
                    boolpart1.Father = beam2;
                    boolpart1.SetOperativePart(cp);

                    if (!boolpart1.Insert())
                        Console.WriteLine("Insert failed!");
                    cp.Delete();
                }
            }
            
            GeometricPlane geometricPlane1 = new GeometricPlane();
            geometricPlane1.Origin = point1 = new Point(point.X + (gap / 2) * vector.X, point.Y + (gap / 2) * vector.Y, point.Z + (gap / 2) * vector.Z);
            geometricPlane1.Normal = vector;
            Point hold = Intersection.LineToPlane(new Line(beam2_centerLine[0] as Point , beam2_centerLine[1] as Point), geometricPlane1);
            Line line1 = Intersection.PlaneToPlane(ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex].Face), ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex + 1].Face));
            Line line2 = Intersection.PlaneToPlane(ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex + 1].Face), ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex + 2].Face));
            Line line3 = Intersection.PlaneToPlane(ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex - 1].Face), ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex - 2].Face));
            Line line4 = Intersection.PlaneToPlane(ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex].Face), ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex - 1].Face));
            Point po1 = GetClosestPointOnLineSegment(hold, Intersection.LineToPlane(line1, ConvertFaceToGeometricPlane(beam1_faces[12].Face)), Intersection.LineToPlane(line1, ConvertFaceToGeometricPlane(beam1_faces[13].Face)));
            Point po2 = GetClosestPointOnLineSegment(hold, Intersection.LineToPlane(line2, ConvertFaceToGeometricPlane(beam1_faces[12].Face)), Intersection.LineToPlane(line2, ConvertFaceToGeometricPlane(beam1_faces[13].Face)));
            Point po3 = GetClosestPointOnLineSegment(hold, Intersection.LineToPlane(line3, ConvertFaceToGeometricPlane(beam1_faces[12].Face)), Intersection.LineToPlane(line3, ConvertFaceToGeometricPlane(beam1_faces[13].Face)));
            Point po4 = GetClosestPointOnLineSegment(hold, Intersection.LineToPlane(line4, ConvertFaceToGeometricPlane(beam1_faces[12].Face)), Intersection.LineToPlane(line4, ConvertFaceToGeometricPlane(beam1_faces[13].Face)));
            countourPoints.Clear();

            double width = CalculateDistanceBetweenFaces(beam2_faces[2].Face, beam2_faces[8].Face);
            foreach (Point po in new List<Point> { po1, po2, po3, po4 }) 
            {
                if(new List<Point> { po1,po4 }.Contains(po))
                {
                    ContourPoint contourPoint = new ContourPoint(po, new Chamfer(width,width, ChamferTypeEnum.CHAMFER_LINE));
                    countourPoints.Add(contourPoint);
                }
                else
                {
                    ContourPoint contourPoint = new ContourPoint(po, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
            }
            ContourPlate cp1 = new ContourPlate();
            
            cp1.Contour.ContourPoints = countourPoints;
            cp1.Profile.ProfileString = "PLT" + width ;
            cp1.Class = "5";
            cp1.Material.MaterialString = "IS2062";
            cp1.Position.Depth = Position.DepthEnum.MIDDLE;
            cp1.Insert();
            
            

            Weld Weld = new Weld();
            Weld.MainObject = cp1;
            Weld.SecondaryObject = beam1;
            Weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
            Weld.LengthAbove = 12;
            Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_SLOT;
            Weld.Insert();

            
            
            return cp1;
        }
        private ArrayList polibeamPlate(Beam beam1, Beam beam2, ContourPlate cp,double gap ,double width1, double width2, double hight, double thickness, double topOffset, string material , bool positionFlag)
        {
            
            List<Face_> face_s = get_faces(cp);
            List<Face> faces = new List<Face>();
            foreach (Face_ f in face_s)
            {
                faces.Add(f.Face);
            } 
            List<Face> cp_faces = faces.OrderByDescending(fa => CalculateFaceArea(fa)).ToList();
            faces.Clear();
            List<Face_> beam1_faces = get_faces(beam1);
            List<Face_> beam2_faces = get_faces(beam2);
            foreach (Face_ f in beam2_faces)
            {
                faces.Add(f.Face); 
            }
            List<Face> beam2_Face = faces.OrderByDescending(fa => CalculateFaceArea(fa)).ToList();
            ArrayList beam1_centerLine = beam1.GetCenterLine(false);
            ArrayList beam2_centerLine = beam2.GetCenterLine(false);
            Face_ face = null;
            bool depthFlag = true;
            Point mid = MidPoint(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point);
            if (Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[0].Face)) < Distance.PointToPlane(mid, ConvertFaceToGeometricPlane(beam1_faces[10].Face)))
            {
                face = beam1_faces[0];
                depthFlag = false;
            }
            else
            {
                face = beam1_faces[10];
                depthFlag = true;
            }
            if (!positionFlag)
                depthFlag = !depthFlag;
            Vector vector = beam1_faces[5].Vector;
            Point point = Get_Points(beam1_faces[5].Face)[0] as Point;
            double diffrence = CalculateDistanceBetweenFaces(beam1_faces[5].Face, beam1_faces[11].Face)/2;
            Point point1 = new Point(point.X - diffrence * vector.X, point.Y - diffrence * vector.Y, point.Z - diffrence * vector.Z);
            GeometricPlane geometricPlane = new GeometricPlane(point1, vector);
            GeometricPlane refference = ConvertFaceToGeometricPlane(face.Face);
            if (!ArePlanesPerpendicular(ConvertFaceToGeometricPlane(beam2_faces[2].Face), ConvertFaceToGeometricPlane(face.Face)))
            {
                GeometricPlane plA = ConvertFaceToGeometricPlane(cp_faces[0]),
                    plB = ConvertFaceToGeometricPlane(cp_faces[1]),
                    pl1 = ConvertFaceToGeometricPlane(beam2_Face[0]),
                    pl2 = ConvertFaceToGeometricPlane(beam2_Face[1]);

                Line line1 = Intersection.PlaneToPlane(plA, pl1),
                    line2 = Intersection.PlaneToPlane(plA, pl2),
                    line3 = Intersection.PlaneToPlane(plB, pl1),
                    line4 = Intersection.PlaneToPlane(plB, pl2);
                
                Point p1 = Intersection.LineToPlane(line1, geometricPlane),
                    p2 = Intersection.LineToPlane(line2, geometricPlane),
                    p3 = Intersection.LineToPlane(line3, geometricPlane),
                    p4 = Intersection.LineToPlane(line4, geometricPlane);
                
                Point center1 = new Point(), center2 = new Point();
                Point poliPointA1 , poliPointA2 , poliPointB1, poliPointB2;
                if (Distance.PointToPoint(p1,p4) < Distance.PointToPoint(p2, p3))
                {
                    center1 = p1; center2 = p4;
                    if(Distance.PointToPlane(center1,refference)< Distance.PointToPlane(center2, refference))
                    {
                        poliPointB2 = FindPointOnLine(center2, p2, width2);
                        poliPointB1 = FindClosestPointOnPlane(pl1, poliPointB2);
                        poliPointA2 = FindPointOnLine(center2 , p3, width1);
                        poliPointA1 = FindClosestPointOnPlane(plA, poliPointA2);
                    }
                    else
                    {
                        poliPointB1 = FindPointOnLine(center1, p3, width2);
                        poliPointB2 = FindClosestPointOnPlane(pl2, poliPointB1);
                        poliPointA1 = FindPointOnLine(center1, p2, width1);
                        poliPointA2 = FindClosestPointOnPlane(plB, poliPointA1);
                    }
                    
                }
                else
                {
                    center1 = p2; center2 = p3;
                    if (Distance.PointToPlane(center1, refference) < Distance.PointToPlane(center2, refference))
                    {
                        poliPointB2 = FindPointOnLine(center2, p1, width2);
                        poliPointB1 = FindClosestPointOnPlane(pl2, poliPointB2);
                        poliPointA2 = FindPointOnLine(center2, p4, width1);
                        poliPointA1 = FindClosestPointOnPlane(plA, poliPointA2);
                    }
                    else
                    {
                        poliPointB1 = FindPointOnLine(center1, p4, width2);
                        poliPointB2 = FindClosestPointOnPlane(pl1, poliPointB1);
                        poliPointA1 = FindPointOnLine(center1, p1, width1);
                        poliPointA2 = FindClosestPointOnPlane(plB, poliPointA1);
                    }
                   
                }
                ArrayList countourPoints = new ArrayList();
                foreach (Point p in new List<Point> {poliPointA1 , center1 ,poliPointB1 })
                {
                    
                    ContourPoint contourPoint = new ContourPoint(p, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                PolyBeam pb = new PolyBeam();
                pb.Contour.ContourPoints = countourPoints;
                pb.Profile.ProfileString = "PLT"+thickness+"*"+hight;
                pb.Position.Depth = Position.DepthEnum.MIDDLE;
                pb.Position.DepthOffset = topOffset;
                pb.Position.Plane =(depthFlag)? Position.PlaneEnum.RIGHT: Position.PlaneEnum.LEFT;
                pb.Position.Rotation = Position.RotationEnum.TOP;
                pb.Material.MaterialString = material;
                pb.Class = "1";
                pb.Insert();
                countourPoints.Clear();
                foreach (Point p in new List<Point> { poliPointA2, center2, poliPointB2 })
                {
                    
                    ContourPoint contourPoint = new ContourPoint(p, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                PolyBeam pb1 = new PolyBeam();
                pb1.Contour.ContourPoints = countourPoints;
                pb1.Profile.ProfileString = "PLT" + thickness + "*" + hight;
                pb1.Position.Depth = Position.DepthEnum.MIDDLE;
                pb1.Position.DepthOffset = topOffset;
                pb1.Position.Plane = (!depthFlag) ? Position.PlaneEnum.RIGHT : Position.PlaneEnum.LEFT;
                pb1.Position.Rotation = Position.RotationEnum.TOP;
                pb1.Material.MaterialString = material;
                pb1.Class = "1";
                pb1.Insert();
                countourPoints.Clear();
                ControlPoint controlPoint = new ControlPoint(new Point(0,0,0));
                controlPoint.Insert();
                return new ArrayList {pb , pb1 };
                
            }
            else
            {
              

                
                vector = face.Vector;
                point = refference.Origin;
                Point origin = new Point(point.X + gap / 2 * vector.X, point.Y + gap / 2 * vector.Y, point.Z + gap / 2 * vector.Z);
                GeometricPlane plane = new GeometricPlane(origin,vector );
                Line line = Intersection.PlaneToPlane(plane, geometricPlane);
                Point p1 = Intersection.LineToPlane(line, ConvertFaceToGeometricPlane(cp_faces[0])),
                    p2 = Intersection.LineToPlane(line, ConvertFaceToGeometricPlane(cp_faces[1])),
                    p1a = FindClosestPointOnPlane(refference,p1),
                    p2a = FindClosestPointOnPlane(refference,p2);
                
                Point po1a = FindPointOnLine(p1, p1a, width1),
                    po1b = FindPointOnLine(p1, p1a, width2 * -1),
                    po2a = FindPointOnLine(p2, p2a, width1),
                    po2b = FindPointOnLine(p2, p2a, width2 * -1);

                ArrayList countourPoints = new ArrayList();
                foreach (Point p in new List<Point> { po1a, p1, po1b })
                {
                    
                    ContourPoint contourPoint = new ContourPoint(p, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                PolyBeam pb = new PolyBeam();
                pb.Contour.ContourPoints = countourPoints;
                pb.Profile.ProfileString = "PLT" + hight + "*" + thickness;
                pb.Position.Depth = Position.DepthEnum.MIDDLE;
                pb.Position.DepthOffset = topOffset;
                pb.Position.Plane = (!depthFlag) ? Position.PlaneEnum.RIGHT : Position.PlaneEnum.LEFT;
                pb.Position.Rotation = Position.RotationEnum.TOP;
                pb.Material.MaterialString = material;
                pb.Class = "1";
                pb.Insert();
                countourPoints.Clear();
                foreach (Point p in new List<Point> { po2a, p2, po2b })
                {
                    ContourPoint contourPoint = new ContourPoint(p, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                PolyBeam pb1 = new PolyBeam();
                pb1.Contour.ContourPoints = countourPoints;
                
                pb1.Profile.ProfileString = "PLT" + hight + "*" + thickness;
                pb1.Position.Depth = Position.DepthEnum.MIDDLE;
                pb1.Position.DepthOffset = topOffset;
                pb1.Position.Plane = (depthFlag) ? Position.PlaneEnum.RIGHT : Position.PlaneEnum.LEFT;
                pb1.Position.Rotation = Position.RotationEnum.TOP;
                pb1.Material.MaterialString = material;
                pb1.Class = "1";
                pb1.Insert();
                return new ArrayList { pb, pb1 };
            }

        }
        private void BoltArray(Part part1, Part part2, Part cp1, Part cp2)
        {
            
            TransformationPlane currentPlane = myModel.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            TransformationPlane part2Plane = new TransformationPlane(part2.GetCoordinateSystem());
            try
            {
                List<Face_> part1Faces = get_faces(part1);
                List<Face_> Part1_Faces = part1Faces.OrderByDescending(fa => CalculateFaceArea(fa)).ToList();
                List<Face_> part2Faces = get_faces(part2);
                List<Face_> Part2_Faces = part2Faces.OrderByDescending(fa => CalculateFaceArea(fa)).ToList();
                ArrayList Part1_Points = Get_Points(Part1_Faces[0].Face);

                BoltArray bA = new BoltArray();
                bA.PartToBeBolted = cp1;

                bA.PartToBoltTo = cp2;
                bA.AddOtherPartToBolt(part1);
                ContourPlate cp = part1 as ContourPlate;
                ArrayList points = cp.Contour.ContourPoints;

                Point cpMid = MidPoint(points[0] as Point, points[2] as Point);
                Point refference = MidPoint(points[3] as Point, points[2] as Point);
                List<Face_> cp1_faces = get_faces(cp1);
                GeometricPlane geometricPlane = new GeometricPlane();
                if (CalculateDistanceBetweenFaces(Part1_Faces[0].Face, cp1_faces[1].Face) > CalculateDistanceBetweenFaces(Part1_Faces[0].Face, cp1_faces[2].Face))
                {
                    geometricPlane = ConvertFaceToGeometricPlane(cp1_faces[1].Face);
                }
                else
                    geometricPlane = ConvertFaceToGeometricPlane(cp1_faces[2].Face);

                bA.BoltSize = _BoltSizeEnum[_BoltSize];
                bA.Tolerance = _BoltToletance;
                bA.BoltStandard = _BoltStandardEnum[_BoltStandard];
                bA.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                bA.CutLength = 105;

                bA.Length = 100;
                bA.ExtraLength = 15;
                bA.ThreadInMaterial = (_BoltThreadMat == 0) ? BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES : BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_NO;

                bA.Position.Depth = Position.DepthEnum.MIDDLE;
                bA.Position.Plane = Position.PlaneEnum.MIDDLE;
                bA.Position.Rotation = Position.RotationEnum.FRONT;

                bA.Bolt = (_FlagBolt == 0) ? true : false;
                bA.Washer1 = (_FlagWasher1 == 0) ? true : false;
                bA.Washer2 = (_FlagWasher2 == 0) ? true : false;
                bA.Washer3 = (_FlagWasher3 == 0) ? true : false;
                bA.Nut1 = (_FlagNut1 == 0) ? true : false;
                bA.Nut2 = (_FlagNut2 == 0) ? true : false;

                double total = 0;
                List<double> doubles = InputConverter(_BA1xText);
                bool flag = false;
                double hold = 0;

                if (doubles == null)
                    bA.AddBoltDistX(0);
                if (_BA1xCount > 0 && doubles != null)
                {
                    if (doubles[0] != 0)
                        bA.AddBoltDistX(0);
                    if (doubles.Count == 1)
                        flag = true;
                    for (int i = 0; i < _BA1xCount - 1; i++)
                    {
                        if (i == doubles.Count - 1)
                        {
                            hold = doubles[i];
                        }
                        if (i >= doubles.Count)
                        {
                            bA.AddBoltDistX(hold);
                            total += hold;
                        }
                        else
                        {
                            bA.AddBoltDistX((flag) ? doubles[0] : doubles[i]);
                            total += (flag) ? doubles[0] : doubles[i];

                        }
                    }
                }
                bA.StartPointOffset.Dx = _BA1OffsetX;
                if (doubles != null)
                    doubles.Clear();
                doubles = InputConverter(_BA1yText);

                if (doubles == null)
                    bA.AddBoltDistY(0);
                if (_BA1yCount > 0 && doubles != null)
                {
                    if (doubles[0] != 0)
                        bA.AddBoltDistY(0);
                    if (doubles.Count == 1)
                        flag = true;
                    for (int i = 0; i < _BA1yCount - 1; i++)
                    {
                        if (i == doubles.Count - 1)
                        {
                            hold = doubles[i];
                        }
                        if (i >= doubles.Count)
                        {
                            bA.AddBoltDistY(hold);

                        }
                        else
                        {
                            bA.AddBoltDistY((flag) ? doubles[0] : doubles[i]);
                        }
                    }
                }


                bA.StartPointOffset.Dy = _BA1OffsetY;
                Point point1 = FindClosestPointOnPlane(geometricPlane, refference),
                    point2 = FindPointOnLine(FindClosestPointOnPlane(geometricPlane, cpMid), point1, total / 2 * -1);

                bA.SecondPosition = point1;
                bA.FirstPosition = point2;
                bA.Insert();


                Face_ face_ = new Face_();
                if (CalculateDistanceBetweenFaces(Part2_Faces[0].Face, cp1_faces[3].Face) > CalculateDistanceBetweenFaces(Part2_Faces[0].Face, cp1_faces[5].Face))
                {
                    face_ = cp1_faces[3];
                }
                else
                    face_ = cp1_faces[5];
                BoltArray bA1 = new BoltArray();
                bA1.PartToBeBolted = cp1;

                bA1.PartToBoltTo = part2;
                //bA1.AddOtherPartToBolt(part2);

                bA1.BoltSize = _BoltSizeEnum[_BoltSize];
                bA1.Tolerance = _BoltToletance;
                bA1.BoltStandard = _BoltStandardEnum[_BoltStandard];
                bA1.BoltType = BoltGroup.BoltTypeEnum.BOLT_TYPE_WORKSHOP;
                bA1.CutLength = 105;

                bA1.Length = 100;
                bA1.ExtraLength = 15;
                bA1.ThreadInMaterial = (_BoltThreadMat == 0) ? BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_YES : BoltGroup.BoltThreadInMaterialEnum.THREAD_IN_MATERIAL_NO;

                bA1.Position.Depth = Position.DepthEnum.MIDDLE;
                bA1.Position.Plane = Position.PlaneEnum.MIDDLE;
                bA1.Position.Rotation = Position.RotationEnum.FRONT;

                bA1.Bolt = (_FlagBolt == 0) ? true : false;
                bA1.Washer1 = (_FlagWasher1 == 0) ? true : false;
                bA1.Washer2 = (_FlagWasher2 == 0) ? true : false;
                bA1.Washer3 = (_FlagWasher3 == 0) ? true : false;
                bA1.Nut1 = (_FlagNut1 == 0) ? true : false;
                bA1.Nut2 = (_FlagNut2 == 0) ? true : false;
                if (doubles != null)
                    doubles.Clear();
                doubles = InputConverter(_BA2xText);
                total = 0;
                flag = false;
                hold = 0;

                if (doubles == null)
                    bA1.AddBoltDistX(0);
                if (_BA2xCount > 0 && doubles != null)
                {
                    if (doubles[0] != 0)
                        bA1.AddBoltDistX(0);
                    if (doubles.Count == 1)
                        flag = true;
                    for (int i = 0; i < _BA2xCount - 1; i++)
                    {
                        if (i == doubles.Count - 1)
                        {
                            hold = doubles[i];
                        }
                        if (i >= doubles.Count)
                        {
                            bA1.AddBoltDistX(hold);
                            total += hold;
                        }
                        else
                        {
                            bA1.AddBoltDistX((flag) ? doubles[0] : doubles[i]);
                            total += (flag) ? doubles[0] : doubles[i];

                        }
                    }
                }
                bA1.StartPointOffset.Dx = _BA2OffsetX;
                if (doubles != null)
                    doubles.Clear();
                doubles = InputConverter(_BA2yText);

                if (doubles == null)
                    bA1.AddBoltDistY(0);
                if (_BA2yCount > 0 && doubles != null)
                {
                    if (doubles[0] != 0)
                        bA1.AddBoltDistY(0);
                    if (doubles.Count == 1)
                        flag = true;
                    for (int i = 0; i < _BA2yCount - 1; i++)
                    {
                        if (i == doubles.Count - 1)
                        {
                            hold = doubles[i];
                        }
                        if (i >= doubles.Count)
                        {
                            bA1.AddBoltDistY(hold);

                        }
                        else
                        {
                            bA1.AddBoltDistY((flag) ? doubles[0] : doubles[i]);
                        }
                    }
                }


                bA1.StartPointOffset.Dy = _BA2OffsetY;

                ArrayList list = Get_Points(face_.Face);
                Point point_refference = new Point();
                if (AreLineSegmentsParallel(list[0] as Point, list[1] as Point, point1, point2))
                {
                    point_refference = MidPoint(list[0] as Point, list[3] as Point);
                }
                else
                    point_refference = MidPoint(list[0] as Point, list[1] as Point);
                Point mid = MidPoint(list[0] as Point, list[2] as Point);


                

                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(part2Plane);
                
                Point lPoint1 = part2Plane.TransformationMatrixToLocal.Transform(currentPlane.TransformationMatrixToGlobal.Transform(FindPointOnLine(mid, point_refference, total / 2 * -1)));
                Point lPoint2 = part2Plane.TransformationMatrixToLocal.Transform(currentPlane.TransformationMatrixToGlobal.Transform(point_refference));
                bA1.FirstPosition = lPoint1;
                bA1.SecondPosition = lPoint2;
                ControlPoint controlPoint = new ControlPoint(bA1.FirstPosition);
                controlPoint.Insert();
                ControlPoint controlPoint1 = new ControlPoint(bA1.SecondPosition);
                controlPoint1.Insert();


                flag = bA1.Insert();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message); 
            }
            finally
            {
                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentPlane);
                
            }
        }
        class Face_
        {
            public Face Face { get; set; }
            public Vector Vector { get; set; }
            public void face_(Face face, Vector vector)
            {
                Face = face;
                Vector = vector;
            }
        }
        private List<Face_> get_faces(Part beam)
        {

            Solid solid = beam.GetSolid();
            FaceEnumerator faceEnumerator = solid.GetFaceEnumerator();
            List<Face_> faces = new List<Face_>();
            while (faceEnumerator.MoveNext())
            {

                Face face = faceEnumerator.Current as Face;
                Vector vector = face.Normal;
                faces.Add(new Face_ { Face = face, Vector = vector });

            }

            return faces;
        }
        private double CalculateDistanceBetweenFaces(Face face1, Face face2)
        {
            // Get the loop vertices of both faces to extract points
            ArrayList face1Vertices = Get_Points(face1);
            ArrayList face2Vertices = Get_Points(face2);

            if (face1Vertices == null || face1Vertices.Count == 0 || face2Vertices == null || face2Vertices.Count == 0)
            {
                throw new ArgumentException("One or both faces do not have vertices.");
            }

            // Initialize the minimum distance to a large value
            double minDistance = double.MaxValue;

            // Loop through all points on face1 and face2 and calculate the distance between each pair
            foreach (Point p1 in face1Vertices)
            {
                foreach (Point p2 in face2Vertices)
                {
                    double distance = Distance.PointToPoint(p1,p2);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance;
        }
        
        private ArrayList Get_Points(Face face)
        {
            ArrayList points = new ArrayList();
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {

                Loop loop = loopEnumerator.Current as Loop;
                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
                while (vertexEnumerator.MoveNext())
                {
                    points.Add(vertexEnumerator.Current);
                }
            }
            return points;
        }
        private static GeometricPlane ConvertFaceToGeometricPlane(Face face)
        {
            ArrayList points = new ArrayList();
            // Get the edges from the face (since 'Points' is not available)
            LoopEnumerator loopEnumerator = face.GetLoopEnumerator();
            while (loopEnumerator.MoveNext())
            {

                Loop loop = loopEnumerator.Current as Loop;
                VertexEnumerator vertexEnumerator = loop.GetVertexEnumerator();
                while (vertexEnumerator.MoveNext())
                {
                    points.Add(vertexEnumerator.Current);
                }
            }

            Point point1 = points[0] as Point;
            Point point2 = points[1] as Point;
            Point point3 = points[2] as Point;



            if (point1 == null || point2 == null || point3 == null)
            {
                throw new ArgumentException("The face does not have sufficient points to define a plane.");
            }

            // Create vectors from the points
            Vector vector1 = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector (cross product of the two vectors)
            Vector normalVector = Vector.Cross(vector1, vector2);
            normalVector.Normalize();

            // Create the geometric plane using point1 and the normal vector
            GeometricPlane geometricPlane = new GeometricPlane(point1, normalVector);

            return geometricPlane;
        }
        private Point MidPoint(Point point, Point point1)
        {
            Point mid = new Point((point.X + point1.X) / 2, (point.Y + point1.Y) / 2, (point.Z + point1.Z) / 2);
            return mid;
        }
        private void GetFaceAxes(Face face, out Vector xAxis, out Vector yAxis)
        {
            Vector normalVector;
            // Get the loop vertices of the face to extract points
            ArrayList vertices = Get_Points(face);

            if (vertices == null || vertices.Count < 3)
            {
                throw new ArgumentException("The face does not have enough vertices to define axes.");
            }

            // Select three distinct points to define the plane and axes
            Point point1 = vertices[0] as Point;
            Point point2 = vertices[1] as Point;
            Point point3 = vertices[2] as Point;

            // Define the X-axis vector as the vector between point1 and point2
            xAxis = new Vector(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            xAxis.Normalize();

            // Define another vector on the face
            Vector vector2 = new Vector(point3.X - point1.X, point3.Y - point1.Y, point3.Z - point1.Z);

            // Calculate the normal vector (cross product of xAxis and vector2)
            normalVector = Vector.Cross(xAxis, vector2);
            normalVector.Normalize();

            // Define the Y-axis vector as the cross product of the normal vector and X-axis vector
            yAxis = Vector.Cross(normalVector, xAxis);
            yAxis.Normalize();
        }
        private static Point FindClosestPointOnPlane(GeometricPlane plane, Point point)
        {
            // Step 1: Get the normal vector of the plane
            Vector normalVector = plane.Normal;

            // Step 2: Find a vector from the plane's origin to the given point
            Vector pointToPlaneVector = new Vector(point.X - plane.Origin.X, point.Y - plane.Origin.Y, point.Z - plane.Origin.Z);

            // Step 3: Project the pointToPlaneVector onto the plane's normal vector
            double distanceFromPointToPlane = pointToPlaneVector.Dot(normalVector); // Dot product to find projection length along the normal

            // Step 4: Calculate the closest point by moving from the point in the opposite direction of the normal by the distance
            Point closestPoint = new Point(
                point.X - distanceFromPointToPlane * normalVector.X,
                point.Y - distanceFromPointToPlane * normalVector.Y,
                point.Z - distanceFromPointToPlane * normalVector.Z
            );

            return closestPoint;
        }
        public static bool IsPointOnPlane(Point point, GeometricPlane plane)
        {
            // Get the point and normal vector defining the plane
            Point pointOnPlane = plane.Origin;
            Vector normalVector = plane.Normal;

            // Create a vector from the point on the plane to the point being tested
            Vector vectorToPoint = new Vector(point.X - pointOnPlane.X, point.Y - pointOnPlane.Y, point.Z - pointOnPlane.Z);

            // Calculate the dot product of this vector and the normal vector of the plane
            double dotProduct = normalVector.Dot(vectorToPoint);

            // If the dot product is close to zero, the point lies on the plane
            return Math.Abs(dotProduct) < 1e-6; // Tolerance for floating point comparison
        }
        private static Point GetClosestPointOnLineSegment(Point point, Point lineStart, Point lineEnd)
        {
            // Vector from line start to the point
            Vector startToPoint = new Vector(point - lineStart);

            // Direction vector of the line segment
            Vector lineDirection = new Vector(lineEnd - lineStart);
            double lineLengthSquared = lineDirection.Dot(lineDirection);

            // Project the point onto the line segment
            double t = startToPoint.Dot(lineDirection) / lineLengthSquared;

            // Clamp t to the range [0, 1] to keep the projection within the segment
            t = Math.Max(0, Math.Min(1, t));

            // Calculate the closest point on the line segment
            return new Point(
                lineStart.X + t * lineDirection.X,
                lineStart.Y + t * lineDirection.Y,
                lineStart.Z + t * lineDirection.Z
            );
        }
        private static bool ArePlanesPerpendicular(GeometricPlane plane1, GeometricPlane plane2)
        {
            // Get the normal vectors of both planes
            Vector normal1 = plane1.Normal;
            Vector normal2 = plane2.Normal;

            // Two planes are perpendicular if their normal vectors' dot product is zero (or near zero)
            double dotProduct = normal1.Dot(normal2);

            // If the dot product is close to zero, the planes are perpendicular
            return Math.Abs(dotProduct) < 1e-6;  // Small threshold for floating-point precision
        }
        private  double CalculateFaceArea(Face face)
        {
            ArrayList facePoints = Get_Points(face); // Assuming this method gets the list of points of the face

            if (facePoints.Count < 3)
                return 0.0; // A face must have at least 3 points to form a polygon

            double totalArea = 0.0;
            Point basePoint = facePoints[0] as Point;

            // Iterate through the face points and form triangles with the base point
            for (int i = 1; i < facePoints.Count - 1; i++)
            {
                Point point1 = facePoints[i] as Point;
                Point point2 = facePoints[i + 1] as Point;

                // Calculate the area of the triangle formed by basePoint, point1, and point2
                totalArea += CalculateTriangleArea(basePoint, point1, point2);
            }

            return totalArea;
        }
        private double CalculateFaceArea(Face_ face)
        {
            ArrayList facePoints = Get_Points(face.Face); // Assuming this method gets the list of points of the face

            if (facePoints.Count < 3)
                return 0.0; // A face must have at least 3 points to form a polygon

            double totalArea = 0.0;
            Point basePoint = facePoints[0] as Point;

            // Iterate through the face points and form triangles with the base point
            for (int i = 1; i < facePoints.Count - 1; i++)
            {
                Point point1 = facePoints[i] as Point;
                Point point2 = facePoints[i + 1] as Point;

                // Calculate the area of the triangle formed by basePoint, point1, and point2
                totalArea += CalculateTriangleArea(basePoint, point1, point2);
            }

            return totalArea;
        }
        private static double CalculateTriangleArea(Point p1, Point p2, Point p3)
        {
            // Create vectors representing two sides of the triangle
            Vector v1 = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            Vector v2 = new Vector(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

            // The area of the triangle is half the magnitude of the cross product of the vectors
            Vector crossProduct = v1.Cross(v2);
            double area = 0.5 * crossProduct.GetLength();
            return area;
        }
        public static LineSegment ConvertLineToSegment(Line line, GeometricPlane plane, double segmentLength)
        {
            // Step 1: Find the intersection point of the line and the plane (center of the segment)
            Point centerPoint = Intersection.LineToPlane(line, plane);

            if (centerPoint == null)
            {
                // No intersection, return null or handle the error
                return null;
            }

            // Step 2: Use Line.Origin as one of the points on the line
            Point originPoint = line.Origin;

            // Step 3: Calculate the direction vector of the line (from origin)
            Vector direction = new Vector(line.Direction.X, line.Direction.Y, line.Direction.Z);
            direction.Normalize();  // Normalize the vector to get unit direction

            // Step 4: Find the half-length of the segment (since the segment is centered at the intersection)
            double halfLength = segmentLength / 2;

            // Step 5: Calculate the start and end points of the line segment
            Point startPoint = new Point(centerPoint.X - direction.X * halfLength,
                                         centerPoint.Y - direction.Y * halfLength,
                                         centerPoint.Z - direction.Z * halfLength);

            Point endPoint = new Point(centerPoint.X + direction.X * halfLength,
                                       centerPoint.Y + direction.Y * halfLength,
                                       centerPoint.Z + direction.Z * halfLength);

            // Step 6: Create the new LineSegment
            LineSegment lineSegment = new LineSegment(startPoint, endPoint);

            return lineSegment;
        }
        public static Point FindPointOnLine(Point startPoint, Point secondPoint, double distance)
        {
            if (distance == 0)
                return startPoint;
            // Step 1: Calculate the direction vector from startPoint to secondPoint
            Vector direction = new Vector(
                secondPoint.X - startPoint.X,
                secondPoint.Y - startPoint.Y,
                secondPoint.Z - startPoint.Z
            );

            // Step 2: Normalize the direction vector
            direction.Normalize();

            // Step 3: Scale the direction vector by the distance
            Vector scaledVector = new Vector(
                direction.X * distance,
                direction.Y * distance,
                direction.Z * distance
            );

            // Step 4: Calculate the new point by adding the scaled vector to the start point
            Point newPoint = new Point(
                startPoint.X + scaledVector.X,
                startPoint.Y + scaledVector.Y,
                startPoint.Z + scaledVector.Z
            );

            return newPoint;
        }
        public static bool CheckAndConvertToMetric(string input, out double convertedValue)
        {
            // Conversion factor: 1 inch = 25.4 mm, 1 foot = 12 inches
            const double inchToMm = 25.4;
            const double footToInches = 12;

            // Regular expression to capture feet and inches (e.g., 12'10")
            string pattern = @"(?<feet>\d+)'(?<inches>\d+)"".*";
            Match match = Regex.Match(input.Trim(), pattern);

            if (match.Success)
            {
                // Extract feet and inches from the input string
                int feet = int.Parse(match.Groups["feet"].Value);
                int inches = int.Parse(match.Groups["inches"].Value);

                // Convert the feet to inches and add the inches
                double totalInches = (feet * footToInches) + inches;

                // Convert the total inches to millimeters
                convertedValue = totalInches * inchToMm;
                return true; // Imperial format
            }
            else if (double.TryParse(input, out convertedValue))
            {
                // The input is assumed to be in millimeters if it's a pure numeric value
                return false; // Metric format
            }
            else
            {
                // Invalid format
                throw new ArgumentException("Invalid input: The input must be either in feet and inches format (e.g., 12'10\") or numeric millimeters.");
            }
        }
        public  bool AreFacesOnSamePlane(Face face1, Face face2)
        {
            ArrayList face1Points = Get_Points(face1);
            ArrayList face2Points = Get_Points(face2);

            if (face1Points.Count < 3 || face2Points.Count < 3)
                throw new ArgumentException("Each face must have at least 3 points to define a plane.");

            // Define the plane of the first face using its first three points
            Point face1Point1 = face1Points[0] as Point;
            Point face1Point2 = face1Points[1] as Point;
            Point face1Point3 = face1Points[2] as Point;

            // Create the normal vector for the plane of face 1
            Vector normal1 = CalculateNormal(face1Point1, face1Point2, face1Point3);

            // Now, check if all points on the second face lie on the same plane as the first face
            foreach (Point point in face2Points)
            {
                if (!IsPointOnPlane(point, face1Point1, normal1))
                {
                    return false; // If one point is not on the plane, they are not on the same plane
                }
            }

            return true; // All points are on the same plane
        }
        private static Vector CalculateNormal(Point p1, Point p2, Point p3)
        {
            Vector v1 = new Vector(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
            Vector v2 = new Vector(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);

            // Cross product of vectors v1 and v2 to get the normal vector
            return v1.Cross(v2).GetNormal();
        }
        private static bool IsPointOnPlane(Point point, Point planePoint, Vector normal)
        {
            Vector pointVector = new Vector(point.X - planePoint.X, point.Y - planePoint.Y, point.Z - planePoint.Z);

            // If the dot product of the normal vector and the vector from planePoint to point is zero, the point is on the plane
            return Math.Abs(pointVector.Dot(normal)) < 1e-6; // Small tolerance for floating point precision
        }
        private List<double> InputConverter(string input)
        {
            if(input == "")
                return null;
            string[] hold = input.Split(' ');
            List<double> output = new List<double>();
            foreach(string s  in hold)
            {
                if(s.Contains('*'))
                {
                    string[] strings = s.Split('*');
                    for(int i = 0;i< int.Parse( strings[0]); i++)
                    {
                        output.Add(double.Parse(strings[1]));
                    }                    
                }
                else
                {
                    output.Add(double.Parse(s));
                }
            }
            return output;
        }
        public static bool AreLineSegmentsParallel(Point line1Start, Point line1End, Point line2Start, Point line2End)
        {
            // Calculate the direction vectors of both line segments
            Vector line1Vector = new Vector(line1End.X - line1Start.X, line1End.Y - line1Start.Y, line1End.Z - line1Start.Z);
            Vector line2Vector = new Vector(line2End.X - line2Start.X, line2End.Y - line2Start.Y, line2End.Z - line2Start.Z);

            // Normalize the vectors to avoid scaling differences
            line1Vector.Normalize();
            line2Vector.Normalize();

            // Check if the cross product of the two vectors is close to zero
            Vector crossProduct = line1Vector.Cross(line2Vector);

            // If the cross product vector is close to (0,0,0), the lines are parallel
            double tolerance = 1e-6; // Tolerance for floating-point comparison
            return crossProduct.GetLength() < tolerance;
        }
        #endregion
    }
}
