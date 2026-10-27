using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace LetterRun2D
{
    public static class TownContentUpdate
    {
        // Applied before tutorial/random-layout snapshots so old scenes need no rebuild.
        public static void Apply(DeliveryGame game)
        {
            // Migrate the previous default also in existing serialized scenes.
            if(game.targetDeliveries==6)game.targetDeliveries=10;
            var letters=new List<LetterItem>();
            LetterItem normal=null;
            foreach(var letter in game.letters)
            {
                if(letter==null)continue;
                letters.Add(letter);if(normal==null && letter.kind==LetterKind.Normal)normal=letter;
            }
            Vector2[] additions={new Vector2(-1.5f,-4.6f),new Vector2(1.5f,-4.6f),new Vector2(-1.4f,-.2f),new Vector2(1.4f,-.2f)};
            int next=0;
            while(normal!=null && letters.Count<12 && next<additions.Length)
            {
                var extra=Object.Instantiate(normal,normal.transform.parent);
                extra.name="Extra normal letter "+(next+1);
                extra.kind=LetterKind.Normal;extra.transform.position=additions[next++];
                extra.ResetLetter();letters.Add(extra);
            }
            game.letters=letters.ToArray();
            foreach(var hazard in game.GetComponentsInChildren<TimedHazard>(true))
            {
                // Absolute scale prevents repeated Play/build application from growing the hazard again.
                hazard.transform.localScale=new Vector3(1.4f,1.4f,1);
            }
            Physics2D.SyncTransforms();
            var rules=game.menuPanel.transform.Find("Rules");
            if(rules!=null)
            {
                var label=rules.GetComponent<Text>();
                if(label!=null)label.text=$"제한 시간 {game.timeLimit:0}초 · 총 {letters.Count}개 중 {game.targetDeliveries}개 배달\n능력 편지도 포함 · 가방은 최대 {game.carryCapacity}개";
            }
        }
    }
}
