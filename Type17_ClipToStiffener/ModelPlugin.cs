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
using Line = Tekla.Structures.Geometry3d.Line;
using Fitting = Tekla.Structures.Model.Fitting;

using TeklaPH;

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

        [StructuresField("PlatePosition")]
        public int PlatePosition;

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
        private int _PlatePosition;

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
                

                var girtCoord = beam1.GetCoordinateSystem();
               
                TransformationPlane currentTransformation = myModel.GetWorkPlaneHandler().GetCurrentTransformationPlane();
                var newWorkPlane = new TransformationPlane(girtCoord);
                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(newWorkPlane);

                ContourPlate cp = ContourFitPart(beam1, beam2, _Gap, _Material);
                
                bool positionFlag  = (_PlatePosition != 0)? true : false;

                ArrayList poliplates = polibeamPlate(beam1,beam2,cp,_Gap,_PlateWidth1,_PlateWidth2,_PlateHight,_Thickness,_TopOffset,_Material, positionFlag);
                
                BoltArray(cp,beam2, poliplates[0] as Part, poliplates[1] as Part);

                Weld Weld = new Weld();
                Weld.MainObject = beam1;
                Weld.SecondaryObject = cp;
                Weld.TypeAbove = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_FILLET;
                Weld.ConnectAssemblies = true;
                Weld.LengthAbove = 12;
                Weld.TypeBelow = BaseWeld.WeldTypeEnum.WELD_TYPE_SLOT;
                Weld.Insert();


                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(currentTransformation);
                

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

            _PlatePosition = Data.PlatePosition;

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
                _TopOffset = -1;
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
                _BA1yText = "50";
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
                _BA2yText = "50";
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
            if (IsDefaultValue(_PlatePosition))
                _PlatePosition = 0;


        }
        
        private ContourPlate ContourFitPart(Beam beam1, Beam beam2 , double gap, string material)
        {
            Faces Faces = new Faces();
            List<Faces.Face_> beam1_faces =Faces. Get_faces(beam1,true);
            ArrayList beam1_centerLine = beam1.GetCenterLine(false);
            ArrayList beam2_centerLine = beam2.GetCenterLine(false);
            Faces.Face_ face = null;
            int  beam1FaceIndex = -1;
            Point mid = MidPoint(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point);
            if (Distance.PointToPlane(mid, Faces.ConvertFaceToGeometricPlane(beam1_faces[0].Face)) < Distance.PointToPlane(mid, Faces.ConvertFaceToGeometricPlane(beam1_faces[10].Face)))
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
            Point point = Faces.Get_Points(face.Face)[0] as Point;
         
            Point point1 = new Point(point.X + gap * vector.X, point.Y + gap * vector.Y, point.Z + gap * vector.Z);
            fitting.Plane.Origin = point1;
            Faces.GetFaceAxes(face.Face, out Vector xAxis, out Vector yAxis);
            fitting.Plane.AxisX = xAxis;
            fitting.Plane.AxisY = yAxis;
            fitting.Father = beam2;
            fitting.Insert();
            List<Faces. Face_> beam2_faces = Faces.Get_faces(beam2,false);
            Line lin = new Line(beam1_centerLine[0] as Point, beam1_centerLine[1] as Point);
            LineSegment lin2 = TeklaPH.Line.FindPerpendicularLineSegment(lin, Faces.ConvertFaceToGeometricPlane(beam1_faces[2].Face), mid, 1000);
            GeometricPlane plane2 = null;
            Faces.Face_ holdFace = null;
            double d = double.MaxValue;
            foreach (var f in beam2_faces)
            {
                ArrayList po = Faces.Get_Points(f.Face);
                if (po.Count == 12)
                {
                  
                    double d2 = Distance.PointToPlane(MidPoint(beam1_centerLine[0] as Point, beam1_centerLine[1] as Point), Faces.ConvertFaceToGeometricPlane(f.Face));
                    if(d>d2)
                    {
                        holdFace = f;
                    }
                }
            }

            

            ArrayList points = Faces.Get_Points(holdFace.Face);
            List<Point> points1 = new List<Point>();
            d = -1;
            foreach(var f in beam2_faces)
            {
                GeometricPlane g = Faces.ConvertFaceToGeometricPlane(f.Face);
                Point point2 = Intersection.LineSegmentToPlane(lin2, g);
                if(point2 != null)
                {
                double d1 = Distance.PointToPoint(mid,point2);
                    if (d < d1)
                    {
                        plane2 = g;
                        d = d1;
                    }
                }
            }
            GeometricPlane plane = Faces.ConvertFaceToGeometricPlane(holdFace.Face), plane1 = plane2;

            foreach (Point p in points)
            {
                
                   if(IsPointOnPlane(p,plane1))
                    points1.Add(p);
               
            }

            Point p1 = Intersection.LineToPlane(new Line(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point), plane);
            Line line = Intersection.PlaneToPlane(plane, Faces.ConvertFaceToGeometricPlane(beam2_faces[5].Face));
             
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
            beam2_faces = Faces.Get_faces(beam2,true);
            if(partCutFlag)
            {
                Point center = Intersection.LineToPlane(new Line(MidPoint(closest, farest), Projection.PointToPlane(mid, plane1)), plane);
                Point point2 = new Point();
                GeometricPlane geometricPlane = Faces.ConvertFaceToGeometricPlane(beam2_faces[4].Face);
                if (IsPointOnPlane(closest, geometricPlane))
                {
                    point2 = Projection.PointToPlane(closest, Faces.ConvertFaceToGeometricPlane(beam2_faces[6].Face));
                }
                else
                    point2 = Projection.PointToPlane(closest, geometricPlane);
                double distance = Faces.CalculateDistanceBetweenFaces(beam2_faces[5].Face, beam2_faces[11].Face);


                GeometricPlane geometricPlane2 = Faces.ConvertFaceToGeometricPlane(beam1_faces[5].Face);


                foreach (Point po in new List<Point> { farest, point2, center })
                {
                    Point point3 = Projection.PointToPlane(po, geometricPlane2);
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
            Line line1 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex].Face), Faces.ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex + 1].Face));
            Line line2 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex + 1].Face), Faces.ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex + 2].Face));
            Line line3 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex - 1].Face), Faces.ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex - 2].Face));
            Line line4 = Intersection.PlaneToPlane(Faces.ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex].Face), Faces.ConvertFaceToGeometricPlane(beam1_faces[beam1FaceIndex - 1].Face));
            Point po1 = Projection.PointToLine(hold,new Line (Intersection.LineToPlane(line1, Faces.ConvertFaceToGeometricPlane(beam1_faces[12].Face)), Intersection.LineToPlane(line1, Faces.ConvertFaceToGeometricPlane(beam1_faces[13].Face))));
            Point po2 = Projection.PointToLine(hold,new Line (Intersection.LineToPlane(line2, Faces.ConvertFaceToGeometricPlane(beam1_faces[12].Face)), Intersection.LineToPlane(line2, Faces.ConvertFaceToGeometricPlane(beam1_faces[13].Face))));
            Point po3 = Projection.PointToLine(hold, new Line(Intersection.LineToPlane(line3, Faces.ConvertFaceToGeometricPlane(beam1_faces[12].Face)), Intersection.LineToPlane(line3, Faces.ConvertFaceToGeometricPlane(beam1_faces[13].Face))));
            Point po4 = Projection.PointToLine(hold, new Line(Intersection.LineToPlane(line4, Faces.ConvertFaceToGeometricPlane(beam1_faces[12].Face)), Intersection.LineToPlane(line4, Faces.ConvertFaceToGeometricPlane(beam1_faces[13].Face))));
            countourPoints.Clear();
            double webThickness = 0.0;

            beam2.GetReportProperty("PROFILE.WEB_THICKNESS", ref webThickness);
            double width = webThickness;
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
            return cp1;
        }
        private ArrayList polibeamPlate(Beam beam1, Beam beam2, ContourPlate cp,double gap ,double width1, double width2, double hight, double thickness, double topOffset, string material,bool position )
        {
            Faces Faces = new Faces();
            TeklaPH.Line _Line = new TeklaPH.Line();
            List<Faces.Face_> face_s = Faces.Get_faces(cp, false);
            List<Face> faces = new List<Face>();
            foreach (Faces.Face_  f in face_s)
            {
                faces.Add(f.Face);
            } 
            List<Face> cp_faces = faces.OrderByDescending(fa => Faces.CalculateFaceArea(fa)).ToList();
            faces.Clear();
            List<Faces.Face_> beam1_faces = Faces.Get_faces(beam1, true);
            List< Faces.Face_> beam2_faces = Faces.Get_faces(beam2, false);
            foreach (Faces.Face_ f in beam2_faces)
            {
                faces.Add(f.Face); 
            }
            List<Face> beam2_Face = faces.OrderByDescending(fa => Faces.CalculateFaceArea(fa)).ToList();
            ArrayList beam1_centerLine = beam1.GetCenterLine(false);
            ArrayList beam2_centerLine = beam2.GetCenterLine(false);
             Faces.Face_ face = null;
            bool depthFlag = true;
            Point mid = MidPoint(beam2_centerLine[0] as Point, beam2_centerLine[1] as Point);
            if (Distance.PointToPlane(mid, Faces.ConvertFaceToGeometricPlane(beam1_faces[0].Face)) < Distance.PointToPlane(mid, Faces.ConvertFaceToGeometricPlane(beam1_faces[10].Face)))
            {
                face = beam1_faces[0];
            }
            else
            {
                face = beam1_faces[10];
            }
            
            Vector vector = beam1_faces[5].Vector;
            Point point = Faces.Get_Points(beam1_faces[5].Face)[0] as Point;
            double diffrence = Faces.CalculateDistanceBetweenFaces(beam1_faces[5].Face, beam1_faces[11].Face)/2;
            Point point1 = new Point(point.X - diffrence * vector.X, point.Y - diffrence * vector.Y, point.Z - diffrence * vector.Z);
            GeometricPlane geometricPlane = new GeometricPlane(point1, vector);
            GeometricPlane refference = Faces.ConvertFaceToGeometricPlane(face.Face);
            if (!ArePlanesPerpendicular(Faces.ConvertFaceToGeometricPlane(beam2_faces[2].Face), Faces.ConvertFaceToGeometricPlane(face.Face)))
            {
                GeometricPlane plA = Faces.ConvertFaceToGeometricPlane(cp_faces[0]),
                    plB = Faces.ConvertFaceToGeometricPlane(cp_faces[1]),
                    pl1 = Faces.ConvertFaceToGeometricPlane(beam2_Face[0]),
                    pl2 = Faces.ConvertFaceToGeometricPlane(beam2_Face[1]);

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
                        poliPointB2 = _Line.FindPointOnLine(center2, p2, width2);
                        poliPointB1 = Projection.PointToPlane(poliPointB2, pl1);
                        poliPointA2 = _Line.FindPointOnLine(center2 , p3, width1);
                        poliPointA1 = Projection.PointToPlane(poliPointA2, plA);
                    }
                    else
                    {
                        poliPointB1 = _Line.FindPointOnLine(center1, p3, width2);
                        poliPointB2 = Projection.PointToPlane(poliPointB1, pl2);
                        poliPointA1 = _Line.FindPointOnLine(center1, p2, width1);
                        poliPointA2 = Projection.PointToPlane(poliPointA1, plB);
                    }
                    depthFlag = true;
                }
                else
                {
                    center1 = p2; center2 = p3;
                    if (Distance.PointToPlane(center1, refference) < Distance.PointToPlane(center2, refference))
                    {
                        poliPointB2 = _Line.FindPointOnLine(center2, p1, width2);
                        poliPointB1 = Projection.PointToPlane(poliPointB2, pl2);
                        poliPointA2 = _Line.FindPointOnLine(center2, p4, width1);
                        poliPointA1 = Projection.PointToPlane(poliPointA2, plA);
                    }
                    else
                    {
                        poliPointB1 = _Line.FindPointOnLine(center1, p4, width2);
                        poliPointB2 = Projection.PointToPlane(poliPointB1, pl1);
                        poliPointA1 = _Line.FindPointOnLine(center1, p1, width1);
                        poliPointA2 = Projection.PointToPlane(poliPointA1, plB);
                    }
                    depthFlag = false;
                }
                Vector vector0 = beam1_faces[5].Vector,
                    vector1;
                Point po = new Point();
                if (vector0.Y > 0)
                   {
                    po = Projection.PointToPlane(center1, Faces.ConvertFaceToGeometricPlane(beam1_faces[5].Face)); 
                    vector1 = beam1_faces[5].Vector;
                }
                else
                { 
                    po = Projection.PointToPlane(center1, Faces.ConvertFaceToGeometricPlane(beam1_faces[11].Face)); 
                    vector1 = beam1_faces[11].Vector;
                }
                if (position)
                    depthFlag = !depthFlag;
                GeometricPlane g1 = new GeometricPlane(po-vector1*(_TopOffset +(hight/2)),vector1);
                ArrayList countourPoints = new ArrayList();
                foreach (Point p in new List<Point> {poliPointA1 , center1 ,poliPointB1 })
                {
                    Point hold = p;
                    if(_TopOffset != -1)
                        hold = Projection.PointToPlane(p,g1 );
                    ContourPoint contourPoint = new ContourPoint(hold, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                PolyBeam pb = new PolyBeam();
                pb.Contour.ContourPoints = countourPoints;
                pb.Profile.ProfileString = "PLT"+thickness+"*"+hight;
                pb.Position.Depth = (depthFlag)? Position.DepthEnum.FRONT : Position.DepthEnum.BEHIND;
                pb.Position.PlaneOffset = 0;
                pb.Position.Plane = Position.PlaneEnum.MIDDLE;
                pb.Position.Rotation = Position.RotationEnum.FRONT;
                pb.Material.MaterialString = material;
                pb.Class = "1";
                pb.Insert();
                countourPoints.Clear();
                foreach (Point p in new List<Point> { poliPointA2, center2, poliPointB2 })
                {
                    Point hold = p;
                    if (_TopOffset != -1)
                        hold = Projection.PointToPlane(p, g1);
                    ContourPoint contourPoint = new ContourPoint(hold, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                PolyBeam pb1 = new PolyBeam();
                pb1.Contour.ContourPoints = countourPoints;
                pb1.Profile.ProfileString = "PLT" + thickness + "*" + hight;
                pb1.Position.Depth =(depthFlag)? Position.DepthEnum.BEHIND : Position.DepthEnum.FRONT;
                pb1.Position.PlaneOffset = 0;
                pb1.Position.Plane =  Position.PlaneEnum.MIDDLE ;
                pb1.Position.Rotation = Position.RotationEnum.FRONT;
                pb1.Material.MaterialString = material;
                pb1.Class = "1";
                pb1.Insert();
                countourPoints.Clear();
               
                return new ArrayList {pb , pb1 };
                
            }
            else
            {
              
                vector = face.Vector;
                point = refference.Origin;
                Point origin = new Point(point.X + gap / 2 * vector.X, point.Y + gap / 2 * vector.Y, point.Z + gap / 2 * vector.Z);
                GeometricPlane plane = new GeometricPlane(origin,vector );
                Line line = Intersection.PlaneToPlane(plane, geometricPlane);
                Point p1 = Intersection.LineToPlane(line, Faces.ConvertFaceToGeometricPlane(cp_faces[0])),
                    p2 = Intersection.LineToPlane(line, Faces.ConvertFaceToGeometricPlane(cp_faces[1])),
                    p1a = Projection.PointToPlane(p1, refference),
                    p2a = Projection.PointToPlane(p2, refference);
                
                Point po1a = _Line.FindPointOnLine(p1, p1a, width1),
                    po1b = _Line.FindPointOnLine(p1, p1a, width2 * -1),
                    po2a = _Line.FindPointOnLine(p2, p2a, width1),
                    po2b = _Line.FindPointOnLine(p2, p2a, width2 * -1);

                ArrayList countourPoints = new ArrayList();
                foreach (Point p in new List<Point> { po1a, p1, po1b })
                {
                    
                    ContourPoint contourPoint = new ContourPoint(p, new Chamfer());
                    countourPoints.Add(contourPoint);
                }
                PolyBeam pb = new PolyBeam();
                pb.Contour.ContourPoints = countourPoints;
                pb.Profile.ProfileString = "PLT" + hight + "*" + thickness;
                pb.Position.Depth = Position.DepthEnum.FRONT;
                pb.Position.PlaneOffset = topOffset;
                pb.Position.Plane = Position.PlaneEnum.MIDDLE;
                pb.Position.Rotation = Position.RotationEnum.FRONT;
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
                pb1.Position.Depth = Position.DepthEnum.BEHIND;
                pb1.Position.PlaneOffset = topOffset;
                pb1.Position.Plane = Position.PlaneEnum.MIDDLE;
                pb1.Position.Rotation = Position.RotationEnum.FRONT;
                pb1.Material.MaterialString = material;
                pb1.Class = "1";
                pb1.Insert();
                return new ArrayList { pb, pb1 };
            }

        }
        private void BoltArray(Part part1, Part part2, Part cp1, Part cp2)
        {
            Faces _Faces = new Faces();
            TeklaPH.Line _Line = new TeklaPH.Line();
            Input input = new Input();
            TransformationPlane currentPlane = myModel.GetWorkPlaneHandler().GetCurrentTransformationPlane();
            TransformationPlane part2Plane = new TransformationPlane(part2.GetCoordinateSystem());
            try
            {
                List<Faces.Face_> part1Faces = _Faces.Get_faces(part1, false);
                List<Faces.Face_> Part1_Faces = part1Faces.OrderByDescending(fa => _Faces.CalculateFaceArea(fa)).ToList();
                List<Faces.Face_> part2Faces = _Faces.Get_faces(part2, false);
                List<Faces.Face_> Part2_Faces = part2Faces.OrderByDescending(fa => _Faces.CalculateFaceArea(fa)).ToList();
                ArrayList Part1_Points = _Faces.Get_Points(Part1_Faces[0].Face);
                ArrayList cp1_centerLine = cp1.GetCenterLine(false);
                BoltArray bA = new BoltArray();
                bA.PartToBeBolted = cp1;

                bA.PartToBoltTo = cp2;
                bA.AddOtherPartToBolt(part1);
                ContourPlate cp = part1 as ContourPlate;
                ArrayList points = cp.Contour.ContourPoints;
                Point cpMid = MidPoint(points[0] as Point, points[2] as Point);
                
                cpMid = Projection.PointToLine(cpMid,new Line( cp1_centerLine[0] as Point, cp1_centerLine[1] as Point));
                Point refference = MidPoint(points[3] as Point, points[2] as Point);
                List<Faces.Face_> cp1_faces = _Faces.Get_faces(cp1, false);
                GeometricPlane geometricPlane = new GeometricPlane();
                if (_Faces.CalculateDistanceBetweenFaces(Part1_Faces[0].Face, cp1_faces[1].Face) > _Faces.CalculateDistanceBetweenFaces(Part1_Faces[0].Face, cp1_faces[2].Face))
                {
                    geometricPlane = Faces.ConvertFaceToGeometricPlane(cp1_faces[1].Face);
                }
                else
                    geometricPlane = Faces.ConvertFaceToGeometricPlane(cp1_faces[2].Face);

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
                bA.Position.Rotation = Position.RotationEnum.TOP;

                bA.Bolt = (_FlagBolt == 0) ? true : false;
                bA.Washer1 = (_FlagWasher1 == 0) ? true : false;
                bA.Washer2 = (_FlagWasher2 == 0) ? true : false;
                bA.Washer3 = (_FlagWasher3 == 0) ? true : false;
                bA.Nut1 = (_FlagNut1 == 0) ? true : false;
                bA.Nut2 = (_FlagNut2 == 0) ? true : false;

                double total = 0;
                List<double> doubles = input.InputConverter(_BA1xText);
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
                bA.EndPointOffset.Dx = _BA1OffsetX;
               
                if (doubles != null)
                    doubles.Clear();
                doubles = input.InputConverter(_BA1yText);

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

                bA.StartPointOffset.Dz = _BA1OffsetY;
                bA.EndPointOffset.Dz = _BA1OffsetY;
                Point point1 = Projection.PointToPlane(refference, geometricPlane),
                    point2 = _Line.FindPointOnLine(Projection.PointToPlane(cpMid, geometricPlane), point1, total / 2 * -1);

                bA.SecondPosition = point1;
                bA.FirstPosition = point2;
                bA.Insert();


                Faces.Face_ face_ = new Faces.Face_();
                if (_Faces. CalculateDistanceBetweenFaces(Part2_Faces[0].Face, cp1_faces[3].Face) > _Faces.CalculateDistanceBetweenFaces(Part2_Faces[0].Face, cp1_faces[5].Face))
                {
                    face_ = cp1_faces[3];
                }
                else
                    face_ = cp1_faces[5];
                BoltArray bA1 = new BoltArray();
                bA1.PartToBeBolted = cp1;
                bA1.PartToBoltTo = cp2;
                bA1.AddOtherPartToBolt(part2); 
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
                doubles = input.InputConverter(_BA2xText);
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
                bA1.EndPointOffset.Dx = _BA2OffsetX;
                if (doubles != null)
                    doubles.Clear();
                doubles = input.InputConverter(_BA2yText);

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
                bA1.EndPointOffset.Dy = _BA2OffsetY;

                ArrayList list = _Faces.Get_Points(face_.Face);
                Point point_refference = new Point();
                if (Parallel.LineSegmentToLineSegment(new LineSegment(list[0] as Point, list[1] as Point),new LineSegment(point1, point2)))
                {
                    point_refference = MidPoint(list[0] as Point, list[3] as Point);
                }
                else
                    point_refference = MidPoint(list[0] as Point, list[1] as Point);
                Point mid = MidPoint(list[0] as Point, list[2] as Point);


                

                myModel.GetWorkPlaneHandler().SetCurrentTransformationPlane(part2Plane);
                
                Point lPoint1 = part2Plane.TransformationMatrixToLocal.Transform(currentPlane.TransformationMatrixToGlobal.Transform(_Line.FindPointOnLine(mid, point_refference, total / 2 * -1)));
                Point lPoint2 = part2Plane.TransformationMatrixToLocal.Transform(currentPlane.TransformationMatrixToGlobal.Transform(point_refference));
                bA1.FirstPosition = lPoint1;
                bA1.SecondPosition = lPoint2;
                


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
        
        private Point MidPoint(Point point, Point point1)
        {
            Point mid = new Point((point.X + point1.X) / 2, (point.Y + point1.Y) / 2, (point.Z + point1.Z) / 2);
            return mid;
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
        
       
        #endregion
    }
}
