using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using CeVIO.Talk.RemoteService2;

using Codeer.Friendly;
using Codeer.Friendly.Dynamic;
using Codeer.Friendly.Windows;
using Codeer.Friendly.Windows.Grasp;
using Codeer.Friendly.Windows.NativeStandardControls;
using RM.Friendly.WPFStandardControls;
using Ong.Friendly.FormsStandardControls;
using System.Windows;
using System.Collections;

namespace talker_bridge64
{
    public class AvatorParam_VREX
    {
        public string AvatorName;
        public int AvatorIndex;
        public AvatorUIParam_VREX AvatorUI;
        public Dictionary<string, EffectValueInfo> VoiceEmotions;
        public Dictionary<string, EffectValueInfo> VoiceEmotions_default;
        public Dictionary<EnumVoiceEffect, EffectValueInfo> VoiceEffects_default;
        public Dictionary<EnumVoiceEffect, EffectValueInfo> VoiceEffects;

        public WindowsAppFriend AvatorProcess;

    }

    public class AvatorUIParam_VREX
    {
        public WPFSlider VolumeSlider;
        public WPFSlider SpeedSlider;
        public WPFSlider PitchSlider;
        public WPFSlider IntonationSlider;
        public WPFSlider ShortPauseSlider;
        public WPFSlider LongPauseSlider;
        public bool WithEmotionParams;
        public Dictionary<string, int> EmotionSliderIndexs;
        public Dictionary<int, WPFSlider> EmotionSliders;

        public int PresetTabIndex;
        public int IndexOnPresetTab;

        public WindowsAppFriend _app;
        public WindowControl uiTreeTop;

        public WindowControl TalkTextBox;
        public FormsButton PlayButton;
        public FormsButton SaveButton;
        public FormsTextBox VolumeText;
        public FormsTextBox SpeedText;
        public FormsTextBox PitchText;
        public FormsTextBox IntonationText;

    }

    public class VREX_Talker
    {
        bool IsAlive;


        // WPF インターフェースを記憶する
        private Dictionary<string, Dictionary<string, WPFSlider>> apply_interface;
        // 話者ごとのデフォルト値を格納するList 要素は 話者名と TalkerPrimitive
        private Dictionary<string, TalkerPrimitive> talkerDefaultParamList;
        // 話者名からAvatorIndexを照合する逆引きDic
        private Dictionary<string, int> gyakubiki;

        public VREX_Talker()
        {
            talkerDefaultParamList = new Dictionary<string, TalkerPrimitive>();
            AvatorParams = new Dictionary<int, AvatorParam_VREX>();
            apply_interface = new Dictionary<string, Dictionary<string, WPFSlider>>();
            gyakubiki = new Dictionary<string, int>();
        }

        private readonly string[] VoiceroidExTitles =
        {
                "VOICEROID＋ 京町セイカ EX",
                "VOICEROID＋ 民安ともえ EX",
                "VOICEROID＋ 結月ゆかり EX",
                "VOICEROID＋ 東北ずん子 EX",
                "VOICEROID＋ 水奈瀬コウ EX",
                "VOICEROID＋ 東北きりたん EX",
                "VOICEROID＋ 琴葉茜",
                "VOICEROID＋ 琴葉葵",
                "VOICEROID＋ 東北ずん子",
                "VOICEROID＋ 鷹の爪 吉田くん EX",
                "VOICEROID＋ 月読アイ EX",
                "VOICEROID＋ 月読ショウタ EX",
                "音街ウナTalk Ex",
                "ギャラ子Talk",
                "ギャラ子 Talk"
        };

        Dictionary<int, AvatorParam_VREX> AvatorParams;


        public int ScanTalker(ArrayList talkerlist, System.Windows.Forms.TextBox log)
        {


            IsAlive = false;

            foreach (AvatorParam_VREX avator in GetVoiceroidProcess())
            {
                AvatorParams.Add(avator.AvatorIndex, avator);
                gyakubiki.Add(avator.AvatorName, avator.AvatorIndex);

                avator.AvatorUI = new AvatorUIParam_VREX();
                var talker = new TalkerPrimitive();


                try
                {
                    avator.AvatorUI._app = avator.AvatorProcess;
                    avator.AvatorUI.uiTreeTop = WindowControl.FromZTop(avator.AvatorUI._app);

                    // Zインデクスはコーディア様の TestAssistantで確認可能。音声効果タブへ切り替えてUI要素を取得する。
                    dynamic VoiceroidExUiTab = new FormsTabControl(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 0, 0));
                    VoiceroidExUiTab.EmulateTabSelect(2);

                    avator.AvatorUI.TalkTextBox = avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 1, 0, 1, 1);
                    avator.AvatorUI.PlayButton = new FormsButton(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 1, 0, 1, 0, 3));
                    avator.AvatorUI.SaveButton = new FormsButton(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 1, 0, 1, 0, 1));
                    avator.AvatorUI.VolumeText = new FormsTextBox(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 0, 0, 0, 0, 8));
                    avator.AvatorUI.SpeedText = new FormsTextBox(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 0, 0, 0, 0, 9));
                    avator.AvatorUI.PitchText = new FormsTextBox(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 0, 0, 0, 0, 10));
                    avator.AvatorUI.IntonationText = new FormsTextBox(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 0, 0, 0, 0, 11));
                }
                catch (Exception e)
                {
                    //                    ThrowException(string.Format(@"{0} {1}", e.Message, e.StackTrace));
                    MessageBox.Show(string.Format(@"{0} {1}", e.Message, e.StackTrace));

                }

                avator.VoiceEffects_default = new Dictionary<EnumVoiceEffect, EffectValueInfo>
                {
                    { EnumVoiceEffect.volume,     new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.volume), 0.0m, 2.0m, 0.01m)},
                    { EnumVoiceEffect.speed,      new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.speed), 0.5m, 4.0m, 0.01m)},
                    { EnumVoiceEffect.pitch,      new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.pitch), 0.5m, 2.0m, 0.01m)},
                    { EnumVoiceEffect.intonation, new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.intonation), 0.0m, 2.0m, 0.01m)}
                };
                avator.VoiceEffects = new Dictionary<EnumVoiceEffect, EffectValueInfo>
                {
                    { EnumVoiceEffect.volume,     new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.volume), 0.0m, 2.0m, 0.01m)},
                    { EnumVoiceEffect.speed,      new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.speed), 0.5m, 4.0m, 0.01m)},
                    { EnumVoiceEffect.pitch,      new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.pitch), 0.5m, 2.0m, 0.01m)},
                    { EnumVoiceEffect.intonation, new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.intonation), 0.0m, 2.0m, 0.01m)}
                };
                avator.VoiceEmotions_default = new Dictionary<string, EffectValueInfo>();
                avator.VoiceEmotions = new Dictionary<string, EffectValueInfo>();
                apply_interface.Add(avator.AvatorName, new Dictionary<string, WPFSlider>());

                talker.effectList.Add("音量");
                talker.VoiceEffects.Add("音量", new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.volume), 0.0m, 2.0m, 0.01m));
                apply_interface[avator.AvatorName].Add("音量", avator.AvatorUI.VolumeSlider);
                talker.effectList.Add("話速");
                talker.VoiceEffects.Add("話速", new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.speed), 0.5m, 4.0m, 0.01m));
                apply_interface[avator.AvatorName].Add("話速", avator.AvatorUI.VolumeSlider);
                talker.effectList.Add("高さ");
                talker.VoiceEffects.Add("高さ", new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.pitch), 0.5m, 2.0m, 0.01m));
                apply_interface[avator.AvatorName].Add("高さ", avator.AvatorUI.VolumeSlider);
                talker.effectList.Add("抑揚");
                talker.VoiceEffects.Add("抑揚", new EffectValueInfo(GetSliderValue(avator.AvatorIndex, EnumVoiceEffect.intonation), 0.0m, 2.0m, 0.01m));
                apply_interface[avator.AvatorName].Add("抑揚", avator.AvatorUI.VolumeSlider);


                // 話者一覧に追加
                //                var talker = new TalkerPrimitive();
                var prefix_str = "VOICEROID+EX ";
                talker.talkerName = prefix_str + avator.AvatorName;
                talker.originalName = avator.AvatorName;
                talker.typeTalker = 5;

                log.Text += talker.talkerName;
                log.Text += " ";

                talkerDefaultParamList.Add(talker.originalName, talker);
                talkerlist.Add(talker);

            }

            IsAlive = AvatorParams.Count != 0;
            return (0);
        }

        private List<AvatorParam_VREX> GetVoiceroidProcess()
        {
            Process[] ProcessList = Process.GetProcesses();
            List<AvatorParam_VREX> VoiceroidProcesses = new List<AvatorParam_VREX>();

//            for (int idxp = 0; idxp < VoiceroidExTitles.Length; idxp++)
//            {
//               string WinTitle1 = VoiceroidExTitles[idxp];
//                string WinTitle2 = WinTitle1 + "*";
//
//                foreach (Process p in ProcessList)
//                {
//                    if ((p.MainWindowHandle != IntPtr.Zero) &&
//                        ((p.MainWindowTitle.Equals(WinTitle1)) || (p.MainWindowTitle.Equals(WinTitle2))))
//                    {
//                        VoiceroidEx.AvatorParam avator = new VoiceroidEx.AvatorParam();
//                        avator.AvatorIndex = idxp;
//                        avator.AvatorName = VoiceroidExTitles[idxp];
//                        avator.AvatorProcess = p;
//
//                        VoiceroidProcesses.Add(avator);
//                        break;
//                    }
//                }
//            }

            int idx = 0;
            foreach (string prodTitle in VoiceroidExTitles)
            {
                string WinTitle1 = prodTitle;
                string WinTitle2 = WinTitle1 + "*";

                foreach (Process p in ProcessList)
                {
//                    uiTreeTop = WindowControl.FromZTop(_app);

                    if ((p.MainWindowHandle != IntPtr.Zero) &&
                         ((p.MainWindowTitle.Equals(WinTitle1)) || (p.MainWindowTitle.Equals(WinTitle2))))
                    {

                        WindowsAppFriend _app;
                        //x86のプロセスでアタッチしてその通信情報を引き継ぐためのバイナリを生成
                        var myProcess = Process.GetCurrentProcess();
                        var binPath = Path.GetTempFileName();
                        Process.Start("Attachx86.exe", $"{p.Id} {myProcess.Id} {binPath}").WaitForExit();

                        //バイナリを元にWindowsAppFriend生成
                        var bin = File.ReadAllBytes(binPath);
                        File.Delete(binPath);

                        //以降はx64のプロセスからx86のプロセスが操作できる
                        _app = new WindowsAppFriend(p.MainWindowHandle, bin);




                        AvatorParam_VREX avator = new AvatorParam_VREX();
                        avator.AvatorIndex = idx;
                        avator.AvatorName = prodTitle;
                        avator.AvatorProcess = _app;

                        VoiceroidProcesses.Add(avator);
                        idx++;

                        break;
                    }
                }
            }

            return VoiceroidProcesses;
        }

        public void Dispose()
        {
//            Dispose(true);
        }

        private int ConvertNametoIndex(Val_Talker talker)
        {
            return gyakubiki[talker.talkerRealName];
        }


        /// <summary>
        /// 指定話者で指定テキストで発声
        /// </summary>
        /// <param name="cid">話者CID</param>
        /// <param name="talkText">発声させるテキスト</param>
        /// <returns>発声にかかった時間（ミリ秒）</returns>
        public double Play(Val_Talker talker, string talkText)
        {
            Stopwatch stopWatch = new Stopwatch();
            int avatorIdx = ConvertNametoIndex(talker);
            AvatorParam_VREX avator = AvatorParams[avatorIdx] as AvatorParam_VREX;
//            avator.Semaphore.Wait();

            if (avator.AvatorUI.PlayButton == null) return 0.0;
            if (avator.AvatorUI.SaveButton == null) return 0.0;
            if (avator.AvatorUI.TalkTextBox == null) return 0.0;

            dynamic VoiceroidExUiTab = new FormsTabControl(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 0, 0));
            VoiceroidExUiTab.EmulateTabSelect(2);

            //            ApplyEffectParameters(avatorIdx);
            //            ApplyEmotionParameters(avatorIdx);

            ApplyEffectParameters(talker, apply_interface[talker.talkerRealName] );

            // 再生中なので再生終了を待つ(音声保存ボタンがEnableになるのを待つ)
            if (!avator.AvatorUI.SaveButton.Enabled)
            {
                while (!avator.AvatorUI.SaveButton.Enabled)
                {
                    Thread.Sleep(10);
                }
            }

            avator.AvatorUI.TalkTextBox["Text"](talkText);
            Thread.Sleep(10);

            stopWatch.Start();

            avator.AvatorUI.PlayButton.EmulateClick();

            // 再生開始を待つ(音声保存ボタンがDisableになるのを待つ)
            if (avator.AvatorUI.SaveButton.Enabled)
            {
                while (avator.AvatorUI.SaveButton.Enabled)
                {
                    Thread.Sleep(10);
                }
            }

//            ResetParameters(talker, apply_interface[talker.talkerRealName], apply_interface_emotion[talker.talkerRealName]);

            // 再生終了を待つ(音声保存ボタンがEnableになるのを待つ)
            if (!avator.AvatorUI.SaveButton.Enabled)
            {
                while (!avator.AvatorUI.SaveButton.Enabled)
                {
                    Thread.Sleep(10);
                }
            }

            ResetParameters(talker, apply_interface[talker.talkerRealName]);
            stopWatch.Stop();
//            avator.Semaphore.Release();

            return stopWatch.ElapsedMilliseconds;
        }
        /*
        /// <summary>
        /// 指定話者で指定テキストで発声
        /// </summary>
        /// <param name="cid">話者CID</param>
        /// <param name="talkText">発声させるテキスト</param>
        public override void PlayAsync(int cid, string talkText)
        {
            int avatorIdx = ConvertAvatorIndex(cid);
            VoiceroidEx.AvatorParam avator = AvatorParams[avatorIdx] as VoiceroidEx.AvatorParam;

            Task.Run(() =>
            {
                avator.Semaphore.Wait();

                if (avator.AvatorUI.PlayButton == null) return;
                if (avator.AvatorUI.SaveButton == null) return;
                if (avator.AvatorUI.TalkTextBox == null) return;

                dynamic VoiceroidExUiTab = new FormsTabControl(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 0, 0));
                VoiceroidExUiTab.EmulateTabSelect(2);

                ApplyEffectParameters(avatorIdx);
                ApplyEmotionParameters(avatorIdx);

                // 再生中なので再生終了を待つ(音声保存ボタンがEnableになるのを待つ)
                if (!avator.AvatorUI.SaveButton.Enabled)
                {
                    while (!avator.AvatorUI.SaveButton.Enabled)
                    {
                        Thread.Sleep(10);
                    }
                }

                avator.AvatorUI.TalkTextBox["Text"](talkText);
                Thread.Sleep(10);

                avator.AvatorUI.PlayButton.EmulateClick();

                // 再生開始を待つ(音声保存ボタンがDisableになるのを待つ)
                if (avator.AvatorUI.SaveButton.Enabled)
                {
                    while (avator.AvatorUI.SaveButton.Enabled)
                    {
                        Thread.Sleep(10);
                    }
                }

                // 再生終了を待つ(音声保存ボタンがEnableになるのを待つ)
                if (!avator.AvatorUI.SaveButton.Enabled)
                {
                    while (!avator.AvatorUI.SaveButton.Enabled)
                    {
                        Thread.Sleep(10);
                    }
                }

                avator.Semaphore.Release();
            });
        }

        /// <summary>
        /// 指定話者で指定テキストで発声した結果をファイルに保存
        /// </summary>
        /// <param name="cid">話者CID</param>
        /// <param name="talkText">発声させるテキスト</param>
        /// <param name="saveFilename">保存先ファイル名</param>
        /// <returns>0.0ミリ秒固定</returns>
        public override double Save(int cid, string talkText, string saveFilename)
        {
            int avatorIdx = ConvertAvatorIndex(cid);
            VoiceroidEx.AvatorParam avator = AvatorParams[avatorIdx] as VoiceroidEx.AvatorParam;

            if (avator.AvatorUI.PlayButton == null) return 0.0;
            if (avator.AvatorUI.SaveButton == null) return 0.0;
            if (avator.AvatorUI.TalkTextBox == null) return 0.0;

            dynamic VoiceroidExUiTab = new FormsTabControl(avator.AvatorUI.uiTreeTop.IdentifyFromZIndex(2, 0, 0, 0, 0));
            VoiceroidExUiTab.EmulateTabSelect(2);

            ApplyEffectParameters(avatorIdx);
            ApplyEmotionParameters(avatorIdx);

            if (!avator.AvatorUI.SaveButton.Enabled)
            {
                while (!avator.AvatorUI.SaveButton.Enabled)
                {
                    Thread.Sleep(10);
                }
            }

            avator.AvatorUI.TalkTextBox["Text"](talkText);
            Thread.Sleep(10);

            avator.AvatorUI.SaveButton.EmulateClick(new Async());

            bool finish_savefileSetup = false;
            while (finish_savefileSetup == false)
            {
                //名前を付けて保存 ダイアログで名前を設定
                var FileDlgs = WindowControl.GetFromWindowText(avator.AvatorUI._app, "音声ファイルの保存");
                try
                {
                    if ((FileDlgs.Length != 0) && (FileDlgs[0].WindowClassName == "#32770"))
                    {
                        // https://github.com/mikoto2000/TTSController UI特定の記述を参照
                        NativeButton OkButton = new NativeButton(FileDlgs[0].IdentifyFromDialogId(1));
                        NativeEdit SaveNameText = new NativeEdit(FileDlgs[0].IdentifyFromZIndex(11, 0, 4, 0, 0));

                        //ファイル名を設定
                        SaveNameText.EmulateChangeText(saveFilename);
                        Thread.Sleep(100);

                        //OKボタンを押す
                        OkButton.EmulateClick(new Async());
                        finish_savefileSetup = true;
                    }
                }
                catch (Exception)
                {
                    //
                }

                Thread.Sleep(10);
            }

            return 0.0;
        }

        /// <summary>
        /// 感情パラメタをデフォルト値に戻す
        /// </summary>
        /// <param name="cid">話者CID</param>
        public override void ResetVoiceEmotion(int cid)
        {
            int avatorIdx = ConvertAvatorIndex(cid);
            AvatorParam avator = AvatorParams[avatorIdx] as AvatorParam;

            foreach (var emotion in avator.VoiceEmotions_default)
            {
                avator.VoiceEmotions[emotion.Key].value = emotion.Value.value;
            }

            ApplyEmotionParameters(avatorIdx);
        }

        /// <summary>
        /// 音声効果をデフォルト値に戻す
        /// </summary>
        /// <param name="cid">話者CID</param>
        public override void ResetVoiceEffect(int cid)
        {
            int avatorIdx = ConvertAvatorIndex(cid);
            AvatorParam avator = AvatorParams[avatorIdx] as AvatorParam;

            foreach (var effect in avator.VoiceEffects_default)
            {
                avator.VoiceEffects[effect.Key].value = effect.Value.value;
            }

            ApplyEffectParameters(avatorIdx);
        }

        public override void Dispose(bool disposing)
        {
            if (Disposed) return;

            if (disposing)
            {
                foreach (var item in AvatorParams)
                {
                    ResetVoiceEffect(item.Key + CidBase);
                    ResetVoiceEmotion(item.Key + CidBase);
                    (item.Value as VoiceroidEx.AvatorParam).AvatorUI._app.Dispose();
                }

                AvatorParams.Clear();

                //throw new NotImplementedException();
            }

            Disposed = true;
        }
        */
        private void ApplyEmotionParameters(int avatorIndex)
        {
            //
        }

        private void ApplyEffectParameters(Val_Talker val_, Dictionary<string, WPFSlider> _interface)
        {
            FormsTextBox TargetTextBox = null;
            double value = 0.00;
            AvatorParam_VREX avator = AvatorParams[ConvertNametoIndex(val_)] as AvatorParam_VREX;

            foreach (var effect in avator.VoiceEffects)
            {
                switch (effect.Key)
                {
                    case EnumVoiceEffect.volume:
                        TargetTextBox = avator.AvatorUI.VolumeText;
                        value = (double)val_.parametor["音量"].val_decimal;
                        break;

                    case EnumVoiceEffect.speed:
                        TargetTextBox = avator.AvatorUI.SpeedText;
                        value = (double)val_.parametor["話速"].val_decimal;
                        break;

                    case EnumVoiceEffect.pitch:
                        TargetTextBox = avator.AvatorUI.PitchText;
                        value = (double)val_.parametor["高さ"].val_decimal;
                        break;

                    case EnumVoiceEffect.intonation:
                        TargetTextBox = avator.AvatorUI.IntonationText;
                        value = (double)val_.parametor["抑揚"].val_decimal;
                        break;
                }

                if (TargetTextBox != null)
                {
                    TargetTextBox.EmulateChangeText(string.Format("{0:0.00}", value));
                }
            }
        }

        private void ResetParameters(Val_Talker val_, Dictionary<string, WPFSlider> _interface)
        {

            TalkerPrimitive defaultParam = talkerDefaultParamList[val_.talkerRealName];

            AvatorParam_VREX avator = AvatorParams[ConvertNametoIndex(val_)] as AvatorParam_VREX;


            foreach (KeyValuePair<string, EffectValueInfo> val__ in defaultParam.VoiceEffects)
            {
                FormsTextBox TargetTextBox = null;
                double value = 0.00;
                if (val__.Key == "音量")
                {
                    TargetTextBox = avator.AvatorUI.VolumeText;
                    value = (double)val__.Value.val;
                }
                else if (val__.Key == "話速")
                {
                    TargetTextBox = avator.AvatorUI.SpeedText;
                    value = (double)val__.Value.val;
                }
                else if (val__.Key == "高さ")
                {
                    TargetTextBox = avator.AvatorUI.PitchText;
                    value = (double)val__.Value.val;

                }
                else if (val__.Key == "抑揚")
                {
                    TargetTextBox = avator.AvatorUI.IntonationText;
                    value = (double)val__.Value.val;
                }
                else
                    break;
            

                if (TargetTextBox != null)
                {
                    TargetTextBox.EmulateChangeText(string.Format("{0:0.00}", value));
                }
            }
        }



        private decimal GetSliderValue(int avatorIdx, EnumVoiceEffect effect)
        {
            decimal value = 0.00m;
            FormsTextBox TargetTextBox = null;
            AvatorParam_VREX avator = AvatorParams[avatorIdx] as AvatorParam_VREX;

            switch (effect)
            {
                case EnumVoiceEffect.volume:
                    TargetTextBox = avator.AvatorUI.VolumeText;
                    break;

                case EnumVoiceEffect.speed:
                    TargetTextBox = avator.AvatorUI.SpeedText;
                    break;

                case EnumVoiceEffect.pitch:
                    TargetTextBox = avator.AvatorUI.PitchText;
                    break;

                case EnumVoiceEffect.intonation:
                    TargetTextBox = avator.AvatorUI.IntonationText;
                    break;
            }

            if (TargetTextBox != null)
            {
                value = Convert.ToDecimal(TargetTextBox.Text);
            }

            return value;
        }

    }

}
