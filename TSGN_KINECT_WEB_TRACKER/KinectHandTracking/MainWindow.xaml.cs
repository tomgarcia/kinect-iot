using Microsoft.Kinect;
using Microsoft.Kinect.Face;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectHandTracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        // Tells if the Current Command has been written to or not.
        static bool command = false;

        static bool readyForCmd = true;

        //Set up stopwatch timer for delay between commands.
        Stopwatch stopwatch = new Stopwatch();

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;

        BodyFrameReader _bodyReader = null;

        // 1) Specify a face frame source and a face frame reader
        FaceFrameSource _faceSource = null;
        FaceFrameReader _faceReader = null;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handlers

        // Primary function. Runs when the window loads in.
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _bodies = new Body[_sensor.BodyFrameSource.BodyCount];

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;

                _bodyReader = _sensor.BodyFrameSource.OpenReader();
                _bodyReader.FrameArrived += BodyReader_FrameArrived;

                // 2) Initialize the face source with the desired features
                _faceSource = new FaceFrameSource(_sensor, 0, FaceFrameFeatures.BoundingBoxInColorSpace |
                                                              FaceFrameFeatures.FaceEngagement |
                                                              FaceFrameFeatures.Glasses |
                                                              FaceFrameFeatures.Happy |
                                                              FaceFrameFeatures.LeftEyeClosed |
                                                              FaceFrameFeatures.MouthOpen |
                                                              FaceFrameFeatures.PointsInColorSpace |
                                                              FaceFrameFeatures.RightEyeClosed);
                _faceReader = _faceSource.OpenReader();
                _faceReader.FrameArrived += FaceReader_FrameArrived;
            }
        }

        // Actively updates information on location and status of the user's face relative to body.
        void BodyReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    frame.GetAndRefreshBodyData(_bodies);

                    Body body = _bodies.Where(b => b.IsTracked).FirstOrDefault();

                    if (!_faceSource.IsTrackingIdValid)
                    {
                        if (body != null)
                        {
                            // 4) Assign a tracking ID to the face source
                            _faceSource.TrackingId = body.TrackingId;
                        }
                    }
                }
            }
        }

        // Manages closing all active processes when exiting the primary window.
        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }

            if (_faceReader != null)
            {
                _faceReader.Dispose();
                _faceReader = null;
            }

            if (_faceSource != null)
            {
                _faceSource.Dispose();
                _faceSource = null;
            }
        }



        // Manages hand gestures.
        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    //camera.Source = frame.ToBitmap();
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    //canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Find the joints
                                Joint handRight = body.Joints[JointType.HandRight];
                                Joint thumbRight = body.Joints[JointType.ThumbRight];

                                Joint handLeft = body.Joints[JointType.HandLeft];
                                Joint thumbLeft = body.Joints[JointType.ThumbLeft];

                                // Draw hands and thumbs
                                //canvas.DrawHand(handRight, _sensor.CoordinateMapper);
                                //canvas.DrawHand(handLeft, _sensor.CoordinateMapper);
                                //canvas.DrawThumb(thumbRight, _sensor.CoordinateMapper);
                                //canvas.DrawThumb(thumbLeft, _sensor.CoordinateMapper);

                                // Find the hand states and print them to the display.
                                string rightHandState = "-";
                                string leftHandState = "-";

                                switch (body.HandRightState)
                                {
                                    case HandState.Open:
                                        rightHandState = "Open";
                                        break;
                                    case HandState.Closed:
                                        rightHandState = "Closed";
                                        break;
                                    case HandState.Lasso:
                                        rightHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        rightHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        rightHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                switch (body.HandLeftState)
                                {
                                    case HandState.Open:
                                        leftHandState = "Open";
                                        break;
                                    case HandState.Closed:
                                        leftHandState = "Closed";
                                        break;
                                    case HandState.Lasso:
                                        leftHandState = "Lasso";
                                        break;
                                    case HandState.Unknown:
                                        leftHandState = "Unknown...";
                                        break;
                                    case HandState.NotTracked:
                                        leftHandState = "Not tracked";
                                        break;
                                    default:
                                        break;
                                }

                                tblRightHandState.Text = rightHandState;
                                tblLeftHandState.Text = leftHandState;

                                // Command Handling Code

                                string cmdChar = "";

                                // Frees the user to make a new command every 1.5 seconds.
                                if (!readyForCmd && (stopwatch.Elapsed.TotalMilliseconds > 1500))
                                {
                                    stopwatch.Reset();
                                    readyForCmd = true;
                                }

                                // Does writing of the commands.
                                if ( leftHandState == "Open" )
                                {
                                    switch (body.HandRightState)
                                    {
                                        case HandState.Open:
                                            cmdChar = "O";
                                            break;
                                        case HandState.Closed:
                                            cmdChar = "C";
                                            break;
                                        case HandState.Lasso:
                                            cmdChar = "L";
                                            break;
                                        default:
                                            break;
                                    }

                                    // If the first command in the string, write without a space to the command string.
                                    if ( cmdChar != "" && readyForCmd )
                                    {
                                        stopwatch.Start();

                                        readyForCmd = false;
                                        
                                        if (!command)
                                        {
                                            tblCommand.Text = cmdChar;
                                            command = true;
                                        }
                                        else
                                        {
                                            tblCommand.Text += (" " + cmdChar);
                                        }
                                    }
                                }
                                // Does command submition with double lasso.
                                else if ( leftHandState == "Lasso" && readyForCmd && rightHandState == "Lasso" )
                                {
                                    command = false;
                                    tblLastCommand.Text = tblCommand.Text;
                                    tblCommand.Text = "-";

                                    stopwatch.Start();
                                    readyForCmd = false;

                                    // Send command combo to web server here (use tblLastCommand.Text)
                                        
                                }
                            }
                        }
                    }


                }
            }
        }

        // Manages facial gestures.
        void FaceReader_FrameArrived(object sender, FaceFrameArrivedEventArgs e)
        {
            // Face
            using (var frame = e.FrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    // 4) Get the face frame result
                    FaceFrameResult result = frame.FaceFrameResult;

                    if (result != null)
                    {
                        // 5) Do magic!

                        // Get the face points, mapped in the color space.
                        var eyeLeft = result.FacePointsInColorSpace[FacePointType.EyeLeft];
                        var eyeRight = result.FacePointsInColorSpace[FacePointType.EyeRight];
                        var nose = result.FacePointsInColorSpace[FacePointType.Nose];
                        var mouthLeft = result.FacePointsInColorSpace[FacePointType.MouthCornerLeft];
                        var mouthRight = result.FacePointsInColorSpace[FacePointType.MouthCornerRight];

                        var glasses = result.FaceProperties[FaceProperty.WearingGlasses];
                        var eyeLeftClosed = result.FaceProperties[FaceProperty.LeftEyeClosed];
                        var eyeRightClosed = result.FaceProperties[FaceProperty.RightEyeClosed];
                        var mouthOpen = result.FaceProperties[FaceProperty.MouthOpen];

                        // Position the canvas UI elements
                        /*
                        Canvas.SetLeft(ellipseEyeLeft, eyeLeft.X - ellipseEyeLeft.Width / 2.0);
                        Canvas.SetTop(ellipseEyeLeft, eyeLeft.Y - ellipseEyeLeft.Height / 2.0);

                        Canvas.SetLeft(ellipseEyeRight, eyeRight.X - ellipseEyeRight.Width / 2.0);
                        Canvas.SetTop(ellipseEyeRight, eyeRight.Y - ellipseEyeRight.Height / 2.0);

                        Canvas.SetLeft(ellipseNose, nose.X - ellipseNose.Width / 2.0);
                        Canvas.SetTop(ellipseNose, nose.Y - ellipseNose.Height / 2.0);

                        Canvas.SetLeft(ellipseMouth, ((mouthRight.X + mouthLeft.X) / 2.0) - ellipseMouth.Width / 2.0);
                        Canvas.SetTop(ellipseMouth, ((mouthRight.Y + mouthLeft.Y) / 2.0) - ellipseMouth.Height / 2.0);
                        ellipseMouth.Width = Math.Abs(mouthRight.X - mouthLeft.X);
                        */
                        
                        // Set some base text for non-conclusive checks to return '-' instead of the prior loop's result.
                        string rightEyeState = "-";
                        string leftEyeState = "-";
                        string mouthState = "-";

                        //if (glasses != DetectionResult.Yes)
                        //{
                        // Checks current state of the user's right eye and displays the according text and/or image.
                        switch (eyeRightClosed)
                        {
                            case DetectionResult.No:
                                rightEyeState = "Open";
                                //ellipseEyeRight.Visibility = Visibility.Visible;
                                break;
                            case DetectionResult.Yes:
                                rightEyeState = "Closed";
                                //ellipseEyeRight.Visibility = Visibility.Collapsed;
                                break;
                            case DetectionResult.Maybe:
                                rightEyeState = "Maybe";
                                //ellipseEyeRight.Visibility = Visibility.Visible;
                                break;
                            default:
                                break;
                        }

                        // Checks current state of the user's left eye and displays the according text and/or image.
                        switch (eyeLeftClosed)
                        {
                            case DetectionResult.No:
                                leftEyeState = "Open";
                                //ellipseEyeLeft.Visibility = Visibility.Visible;
                                break;
                            case DetectionResult.Yes:
                                leftEyeState = "Closed";
                                //ellipseEyeLeft.Visibility = Visibility.Collapsed;
                                break;
                            case DetectionResult.Maybe:
                                leftEyeState = "Maybe";
                                //ellipseEyeLeft.Visibility = Visibility.Visible;
                                break;
                            default:
                                break;
                        }
                        //}
                        /*
                        else
                        {
                            rightEyeState = "Please Remove Glasses";
                            leftEyeState = "Please Remove Glasses";
                        }
                        */

                        // Checks current state of the user's mouth and displays the according text and/or image.
                        switch (mouthOpen)
                        {
                            case DetectionResult.Yes:
                                mouthState = "Open";
                                //ellipseMouth.Height = 50.0;
                                break;
                            case DetectionResult.No:
                                mouthState = "Closed";
                                //ellipseMouth.Height = 20.0;
                                break;
                            case DetectionResult.Maybe:
                                mouthState = "Maybe";
                                //ellipseMouth.Height = 50.0;
                                break;
                            default:
                                break;
                        }

                        // Updates the text values.
                        tblRightEyeState.Text = rightEyeState;
                        tblLeftEyeState.Text = leftEyeState;
                        tblMouthState.Text = mouthState;
                    }
                }
            }
        }
        

        #endregion
    }
}
