using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using rnd = UnityEngine.Random;
using KModkit;
public class AnotherKeypadModule : MonoBehaviour {
    public KMSelectable[] keys;
    public MeshRenderer[] leds;
    public TextMesh[] texts;
    public Material[] mats;
    string[] table = new string[] { "৶ᱨㄙᱩꝬչﾁ〆ծՀԷゝ", "ՀㄙծԷꝬゝﾁ৶ᱩᱨ〆չ", "ﾁԷゝչㄙծՀᱨ৶〆Ꝭᱩ", "〆Հչ৶ԷﾁゝꝬծᱩᱨㄙ"};
    int[] indices = new int[4];
    List<int> order;
    int stage;
    public KMBombModule module;
    public KMAudio sound;
    int moduleId;
    static int moduleIdCounter = 1;
    bool solved;
    bool striking;
    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < 4; i++)
        {
            int j = i;
            keys[j].OnInteract += delegate { PressKey(j); return false; };
        }
        module.OnActivate += delegate { Activate(); };
    }
    void Activate () {
        stage = 0;
        PickIndices();
        order = indices.ToList();
        order.Sort();
        for (int i = 0; i < 4; i++) texts[i].text = table[i][indices[i]].ToString();
        Debug.LogFormat("[Another Keypad Module #{0}] The symbols on the module are {1}, {2}, {3}, and {4}.",moduleId, table[0][indices[0]], table[1][indices[1]], table[2][indices[2]], table[3][indices[3]]);
        Debug.LogFormat("[Another Keypad Module #{0}] The symbols in the order they should be pressed are {1}, {2}, {3}, and {4}.", moduleId, table[Array.IndexOf(indices, order[0])][order[0]], table[Array.IndexOf(indices, order[1])][order[1]], table[Array.IndexOf(indices, order[2])][order[2]], table[Array.IndexOf(indices, order[3])][order[3]]);
    }
	
	void PickIndices()
    {
        for (int i = 0; i < 4; i++) { indices[i] = rnd.Range(0, 12); }
        if (indices[0] == indices[1] || indices[1] == indices[2] || indices[0] == indices[2] || indices[0] == indices[3] || indices[1] == indices[3] || indices[2] == indices[3]) { PickIndices(); }
    }

	void PressKey (int index) {
		if (!solved && !striking)
        {
            keys[index].AddInteractionPunch();
            sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, transform);
            Debug.LogFormat("[Another Keypad Module #{0}] You pressed {1}.", moduleId, texts[index].text);
            if (indices[index] == order[stage])
            {
                Debug.LogFormat("[Another Keypad Module #{0}] That was correct.", moduleId);
                leds[index].material = mats[1];
                stage++;
                if(stage == 4)
                {
                    Debug.LogFormat("[Another Keypad Module #{0}] Module solved.", moduleId);
                    sound.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    module.HandlePass();
                    solved = true;
                }
            }
            else
            {
                Debug.LogFormat("[Another Keypad Module #{0}] That was incorrect. Strike!", moduleId);
                StartCoroutine(Strike(index));
            }
        }
	}
    IEnumerator Strike(int index)
    {
        int count = 0;
        striking = true;
        while (count < 3)
        {
            leds[index].material = mats[2];
            yield return new WaitForSeconds(0.3f);
            leds[index].material = mats[0];
            yield return new WaitForSeconds(0.3f);
            count++;
        }
        module.HandleStrike();
        foreach (MeshRenderer i in leds) i.material = mats[0];
        striking = false;
        Activate();
    }
#pragma warning disable 414
    private string TwitchHelpMessage = "use '!{0} 1234' to press the keys in reading order.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        string validcmds = "1234";
        if (command.Contains(' '))
        {
            yield return "sendtochaterror @{0}, invalid command.";
            yield break;
        }
        else
        {
            for (int i = 0; i < command.Length; i++)
            {
                if (!validcmds.Contains(command[i]))
                {
                    yield return "sendtochaterror Invalid command.";
                    yield break;
                }
            }
            for (int i = 0; i < command.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (command[i] == validcmds[j])
                    {
                        yield return null;
                        yield return new WaitForSeconds(1f);
                        keys[j].OnInteract();
                    }
                }
            }
        }
    }
}
