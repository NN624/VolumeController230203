using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Timers;
using System.Threading;


namespace VolumeController
{
    public partial class Form1 : Form
    {
        
        //private VolumeController vc = new VolumeController();

        private static System.Timers.Timer aTimer;
        public static bool enable_timer = false;
        public static bool enable_timermax = false;
        public static bool start_stop = false;
        public static bool media_roop = false;

        MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
        public MMDevice device;
        int root_max = 0;
        int sum_track = 0;
        int proint = 0;
        int timevolume = 0;

        

        Form2 form2 = new Form2();




        // サンプル再生用の準備
        WaveOutEvent waveOut = new WaveOutEvent();
        AudioFileReader afr = new AudioFileReader("../../../../boice.mp3");
        //AudioFileReader afr = new AudioFileReader("C:\\Users\\s192163\\Desktop\\卒業研究\\VolumeController-master\\VolumeController-master\\boice.mp3");
        //AudioFileReader afr = new AudioFileReader("C:\\Users\\s192034.TSITCL\\OneDrive - Cyber University\\School\\卒業研究\\GraduationResearch\\VolumeController\\VolumeController\\5khz-6db-20sec.wav");
        public Form1()
        {

            InitializeComponent();


            button1.BackColor = Color.White;
            button1.BackgroundImageLayout = ImageLayout.Zoom;
            button1.BackgroundImage = System.Drawing.Image.FromFile("../../../../btsaisei.png");

            device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            // 現在の音量を取得して、初期値として設定

            // デバイスの一覧を取得
            comboBox1.Items.AddRange(GetDevices());
            // デバイスのデフォルト設定
            comboBox1.SelectedIndex = 0;

            form2.Show();




        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            // 再生位置が最後まで来たら初めからにする
            int SampleVoice_last = 4688832;
            if (afr.Position >= SampleVoice_last)
            {
                afr.Position = 0;
            }


            proint = (int)(Math.Round(device.AudioMeterInformation.MasterPeakValue * 100));


            form2.progressBar2.Value = (int)(Math.Round(device.AudioMeterInformation.MasterPeakValue * 100));
            form2.progres_value.Text = System.Convert.ToString(proint);


            if (comboBox1.SelectedItem != null && start_stop == true)
            {

                var device = (MMDevice)comboBox1.SelectedItem;

                // 音量を取得
                var volume = GetVolume();

                sum_track = (int)(GetVolume() * proint);

                form2.sum_trackBar.Value = (int)(GetVolume() * proint);
                form2.label11.Text = System.Convert.ToString(GetVolume() * proint);
                form2.pcvln.Text = System.Convert.ToString(GetVolume());



                // 最大の音量を制限
                if ((sum_track > root_max + 1500) && proint != 0)
                {

                    SetVolume(timevolume);


                }
                else if ((sum_track > root_max) && proint != 0 && enable_timermax == false)
                {

                    SetTimerMax();

                    SetVolume(volume - 1);

                }


                // 最小の音量を制限
                if (sum_track < root_max && proint != 0 && enable_timer == false)
                {

                    SetTimer();

                    SetVolume(volume + 2);

                }

                if ((proint == 0) && volume != 0)
                {
                    SetVolume(volume);

                }

            }
        }


        private void media_btn_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("デバイスを選択してください", "キャプション");
            }
            else
            {
                //media_roop = true;
                if (waveOut.PlaybackState == PlaybackState.Stopped)
                {
                    waveOut.Init(afr);
                    waveOut.Play();
                    media_btn.Text = "停止";
                   
                }
                else if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                    media_btn.Text = "サンプル再生";

                }
            }


        }


        // 音量の短時間連続上昇を防ぐ
        private static void SetTimer()
        {
            // 0.4秒
            aTimer = new System.Timers.Timer(200);
            enable_timer = true;
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;

        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            enable_timer = false;
            Debug.WriteLine("stop");
        }

        //最大音量の急な音量減少を防ぐ
        private static void SetTimerMax()
        {
            // 0.0１秒
            aTimer = new System.Timers.Timer(5);
            enable_timermax = true;
            aTimer.Elapsed += OnTimedEventMax;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;

        }
        private static void OnTimedEventMax(Object source, ElapsedEventArgs e)
        {
            enable_timermax = false;
            Debug.WriteLine("stop");
        }

        // メソッド:音量の変更
        public int SetVolume(int volume)
        {
            // 音量を変更（範囲：0.0～1.0）
            if (volume < 0)
            {
                volume = 0;
            }
            else if (volume > 100)
            {
                volume = 100;
            }
            device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)volume / 100.0f);


            return GetVolume();
        }

        // メソッド：音量の取得 return：0.00～1.00 
        public int GetVolume()
        {
            return (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
        }


        // メソッド：デバイスの一覧を取得
        public object[] GetDevices()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
            return devices.ToArray();
        }


        private void mid_btn_Click_1(object sender, EventArgs e)
        {
            // 現在の音量を取得して、初期値として設定
            label6.Text = System.Convert.ToString(GetVolume());
            //20の部分はprogressBar1の値を固定にしているところ
            timevolume = GetVolume();

            root_max = (int)(GetVolume() * 30);

            //form2に送る値
            form2.root_maxtrackBar.Value = (int)(GetVolume() * 30);
            form2.label10.Text = System.Convert.ToString(GetVolume() * 30);
            form2.pcvl.Text = System.Convert.ToString(GetVolume());


        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (start_stop == false)
            {
                start_stop = true;
                //停止ボタンの画像パス
                button1.BackgroundImage = System.Drawing.Image.FromFile("../../../../btteishi.png");

            }
            else
            {
                start_stop = false;
                //再生ボタンの画像パス
                button1.BackgroundImage = System.Drawing.Image.FromFile("../../../../btsaisei.png");
            }
            
        }
    }

}