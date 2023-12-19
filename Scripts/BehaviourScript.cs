using HuggingFace.API;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BehaviourScript : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Material normal;
    [SerializeField] private Material shiny;

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;
    private bool selected;

    // Start is called before the first frame update
    private void Start() {
        startButton.onClick.AddListener(StartRecording);
        stopButton.onClick.AddListener(StopRecording);
        stopButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (recording && Microphone.GetPosition(null) >= clip.samples && selected) {
            StopRecording();
        }
    }

    public void Act(string action) {
        if (action.Contains("Jump") || action.Contains("JUMP") || action.Contains("jump")) {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 10, ForceMode.Impulse);
        } else if (action.Contains("die") || action.Contains("Die") || action.Contains("DIE")) {
            if (tag == "ar1") {
                GameObject.FindGameObjectWithTag("body1").GetComponent<SkinnedMeshRenderer>().enabled = false;
            } else {
                GameObject.FindGameObjectWithTag("body2").GetComponent<SkinnedMeshRenderer>().enabled = false;
            }
        }
    }

    public void Selected() {
        selected = true;
        if (tag == "ar1") {
            GameObject.FindGameObjectWithTag("body1").GetComponent<SkinnedMeshRenderer>().material = shiny;
        } else {
            GameObject.FindGameObjectWithTag("body2").GetComponent<SkinnedMeshRenderer>().material = shiny;
        }
    }

    public void notSelected() {
        selected = false;
        if (tag == "ar1") {
            GameObject.FindGameObjectWithTag("body1").GetComponent<SkinnedMeshRenderer>().material = normal;
        } else {
            GameObject.FindGameObjectWithTag("body2").GetComponent<SkinnedMeshRenderer>().material = normal;
        }
    }

    private void StartRecording() {
        if (selected) {
            text.color = Color.white;
            text.text = "Recording...";
            startButton.interactable = false;
            stopButton.interactable = true;
            clip = Microphone.Start(null, false, 3, 44100);
            recording = true;
        }
    }

    private void StopRecording() {
        if (selected) {
            var position = Microphone.GetPosition(null);
            Microphone.End(null);
            var samples = new float[position * clip.channels];
            clip.GetData(samples, 0);
            bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
            recording = false;
            SendRecording();
        }
    }

    private void SendRecording() {
        text.color = Color.yellow;
        text.text = "Sending...";
        stopButton.interactable = false;
        HuggingFaceAPI.AutomaticSpeechRecognition(bytes, response => {
            text.color = Color.white;
            text.text = response;
            startButton.interactable = true;
            this.Act(response);
        }, error => {
            text.color = Color.red;
            text.text = error;
            startButton.interactable = true;
        });
    }

    private byte[] EncodeAsWAV(float[] samples, int frequency, int channels) {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2)) {
            using (var writer = new BinaryWriter(memoryStream)) {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples) {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }
}
