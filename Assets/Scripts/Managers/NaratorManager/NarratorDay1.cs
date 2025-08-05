using UnityEngine;
using System.Collections;

public class NarratorDay1 : NarratorBase
{
    [System.Obsolete]
    public override IEnumerator Narrate()
    {
        ResetUIState();

        switch (NarratorManager.Instance.currentTime)
        {
            case TimeOfDay.Night:
                yield return StartCoroutine(PlayNightSequence());
                break;
        }
    }


    [System.Obsolete]
    private IEnumerator PlayNightSequence()
    {
        TimeManager.instance.TimeOfDay = 1.0f;
        AppearObjects();
        yield return StartCoroutine(MoveAgentToMovementPosition(CharacterType.Bidan, 0));
        SetCharacterSpawn(CharacterType.Mother, 0);  // Ibu ke kasur
        // SetCharacterSpawn(CharacterType.Father, 0);  // Ayah ke kursi  
        // SetCharacterSpawn(CharacterType.Bidan, 0);   // Bidan posisi A
        // SetCharacterSpawn(CharacterType.Baby, 0);    // Baby posisi awal
        CloseEyes();
        // Memainkan animasi ibu sedang duduk, ayah sedang duduk
        Debug.Log("Playing sitting animations for mother and father.");
        PlayCharacterAnimation(CharacterType.Mother, "Sit");
        PlayCharacterAnimation(CharacterType.Father, "Sit");
        Debug.Log("Playing narration for Day 1 Night sequence.");
        
        yield return new WaitForSeconds(1f);
        uiElements.narratorText.text = "Day 1\nKelahiran";
        yield return new WaitForSeconds(5f);
        uiElements.narratorText.gameObject.SetActive(false);

        bool seq1Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/DalamPerut/Seq1DalamPerut", 
            () => { seq1Complete = true; });
        yield return new WaitUntil(() => seq1Complete);

        yield return new WaitForSeconds(0.3f);

        PlayAudio("baby_crying");

        yield return new WaitForSeconds(1f);

        bool seq2Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/DalamPerut/Seq2Terlahir", 
            () => { seq2Complete = true; });
        yield return new WaitUntil(() => seq2Complete);

        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutAudio(audioSource, 4f)); 
        }
        yield return new WaitForSeconds(3f);
      
        bool seq3Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq3Kesehatan", 
            () => { seq3Complete = true; });
        yield return new WaitUntil(() => seq3Complete);

        yield return new WaitForSeconds(1f);

        bool seq4Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq4Kesadaran", 
            () => { seq4Complete = true; });
        yield return new WaitUntil(() => seq4Complete);

        yield return new WaitForSeconds(1f);

        yield return new WaitForSeconds(0.5f);
        Debug.Log("Opening eyes");
        FadeOpenEyes();

        bool seq5Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq5MembukaMata", 
            () => { seq5Complete = true; });
        yield return new WaitUntil(() => seq5Complete);

        yield return new WaitForSeconds(1f);
        // pergeseran bayi makin dekat ke ibu
        yield return StartCoroutine(MoveCharacterToPosition(CharacterType.Baby, 1, 2f));
        
        // Memainkan animasi
        
        yield return new WaitForSeconds(2f);

        bool seq6Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq6Makanan", 
            () => { seq6Complete = true; });
        yield return new WaitUntil(() => seq6Complete);

        yield return new WaitForSeconds(1f);

        // navmesh agent bidan
        // bidan akan bergerak dengan posisi yang telah ditentukan
        // Setelah sampai posisi yang ditentukan bidan berpindah di ruang keluarga
        Debug.Log("Closing eyes");
        FadeCloseEyes();

        yield return new WaitForSeconds(1f);
        bool seq7Complete = false;
        dialogGameManager.StartCoreGame("GameData/Dialog/Day1/KamarOrtu/Seq7Nama", 
            () => { seq7Complete = true; });
        yield return new WaitUntil(() => seq7Complete);
        yield return new WaitForSeconds(1f);

        Debug.Log("Sequence 7 complete, closing narration.");
    }

}
