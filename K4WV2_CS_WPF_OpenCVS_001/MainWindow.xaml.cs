using System;
using System.Windows;
using Microsoft.Kinect;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections;
using System.Collections.Generic;

namespace K4WV2_CS_WPF_OpenCVS_001
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //
        KinectSensor kinect;

        BodyFrameReader bodyFrameReader;
        Body[] bodies;
        List<Double> XL = new List<Double>();
        List<Double> YL = new List<Double>();
        List<Double> ZL = new List<Double>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (bodyFrameReader!=null)
            {
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }
            if (kinect!=null)
            {
                kinect.Close();
                kinect = null;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Kinectを開く
                kinect = KinectSensor.GetDefault();
                kinect.Open();
                Console.WriteLine("Kinect Opened");

                //ボディーリーダーを開く
                bodyFrameReader = kinect.BodyFrameSource.OpenReader();
                bodyFrameReader.FrameArrived += bodyFrameReader_FrameArrived;

                //Bodyを入れる配列を作る
                bodies = new Body[kinect.BodyFrameSource.BodyCount];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Close();
            }
        }

        private void bodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            UpdateBodyFrame(e);
            DrawBodyFrame();
        }

        //ボディの表示
        private void DrawBodyFrame()
        {
            try {
                if (CanvasBody.Children.Count>3)
                {
                    CanvasBody.Children.RemoveRange(3, 100);
                }

                //追跡しているBodyのみループする
                if (true)
                {
                    foreach (var body in bodies.Where(bb => bb.IsTracked))
                    {
                        Console.WriteLine("ball");
                        foreach (var joint in body.Joints)
                        {
                            //手の位置が追跡状態
                            if (joint.Value.TrackingState == TrackingState.Tracked)
                            {
                                DrawEllipse(joint.Value, 10, Brushes.Blue);
                            }
                            //手の位置が推測状態
                            else if (joint.Value.TrackingState == TrackingState.Inferred)
                            {
                                DrawEllipse(joint.Value, 10, Brushes.Red);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }

        }

        private void DrawEllipse(Joint joint, int R, SolidColorBrush brush)
        {
            var ellipse = new Ellipse()
            {
                Width = R,
                Height = R,
                Fill = brush,
            };

            //カメラ座標系をデプス座標系に変換する
            var point = kinect.CoordinateMapper.MapCameraPointToDepthSpace(joint.Position);
            if ((point.X<0)||(point.Y<0))
            {
                return;
            }

            //ラベルにミギーの座標を表示
            if (joint.JointType==JointType.HandTipRight)
            {
                XL.Add((Int16)Math.Round(joint.Position.X * 1000));
                YL.Add((Int16)Math.Round(joint.Position.Y * 1000));
                ZL.Add((Int16)Math.Round(joint.Position.Z * 1000));
                if (XL.Count>10)
                {
                    XL.RemoveAt(0);
                    YL.RemoveAt(0);
                    ZL.RemoveAt(0);
                }
                label_x.Content = (Int16)Math.Round(XL.Average());
                label_y.Content = (Int16)Math.Round(YL.Average());
                label_z.Content = (Int16)Math.Round(ZL.Average());
            }

            //Depth座標系で円を配置する
            Canvas.SetLeft(ellipse, point.X - (R / 2));
            Canvas.SetTop(ellipse, point.Y - (R / 2));

            CanvasBody.Children.Add(ellipse);
        }

        //ボディの更新
        private void UpdateBodyFrame(BodyFrameArrivedEventArgs e)
        {
            using (var bodyFrame=e.FrameReference.AcquireFrame())
            {
                if (bodyFrame==null)
                {
                    return;
                }

                //ボディーデータを取得する
                bodyFrame.GetAndRefreshBodyData(bodies);
            }
        }
    }
}
