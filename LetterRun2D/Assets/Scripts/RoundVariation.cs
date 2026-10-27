using System;
using System.Collections.Generic;
using UnityEngine;

namespace LetterRun2D
{
    // Changes only at round start. Restart re-rolls; the tutorial never calls Apply.
    public class RoundVariation : MonoBehaviour
    {
        [Header("Round variation")]
        public bool randomizeRounds = true;
        [Tooltip("0: a new random seed each round. A positive number reproduces a layout for testing.")]
        public int fixedSeed;
        [Min(1)] public float minimumCarMultiplier = 1.2f;
        [Min(1)] public float maximumCarMultiplier = 1.55f;
        [Min(.5f)] public float minimumLetterSpacing = 1.1f;
        [Header("Safe candidate positions for the default 2D town")]
        public Vector2[] lowerCandidates = {
            new Vector2(-2.8f,-4.5f),new Vector2(-1.5f,-4.6f),new Vector2(1.5f,-4.6f),new Vector2(2.8f,-4.5f),
            new Vector2(-2.8f,-2.7f),new Vector2(-1.5f,-2.7f),new Vector2(1.5f,-2.7f),new Vector2(2.8f,-2.7f)};
        public Vector2[] middleCandidates = {
            new Vector2(-2.8f,-.2f),new Vector2(-1.4f,-.2f),new Vector2(0,-.2f),new Vector2(1.4f,-.2f),new Vector2(2.8f,-.2f),
            new Vector2(-2.9f,.9f),new Vector2(0,.9f),new Vector2(2.9f,.9f)};
        public Vector2[] upperCandidates = {
            new Vector2(-2.8f,3.1f),new Vector2(-1.4f,3.1f),new Vector2(0,3.1f),new Vector2(1.4f,3.1f),new Vector2(2.8f,3.1f),
            new Vector2(-2.8f,4.4f),new Vector2(2.8f,4.4f)};
        public int LastSeed { get; private set; }
        private DeliveryGame game;
        private Vector3[] originalLetters;
        private TimedHazard[] hazards;
        private Vector4[] originalHazards;
        private Mailbox mailbox;
        private int previousSeed;

        public void Initialize(DeliveryGame owner)
        {
            game = owner;
            originalLetters = new Vector3[game.letters.Length];
            for(int i=0;i<game.letters.Length;i++) if(game.letters[i]!=null)originalLetters[i]=game.letters[i].transform.position;
            hazards=game.GetComponentsInChildren<TimedHazard>(true);
            originalHazards=new Vector4[hazards.Length];
            for(int i=0;i<hazards.Length;i++)originalHazards[i]=new Vector4(hazards[i].safeDuration,hazards[i].warningDuration,hazards[i].dangerDuration,hazards[i].initialDelay);
            mailbox=game.GetComponentInChildren<Mailbox>(true);
        }
        public void Restore()
        {
            if(game==null)return;
            for(int i=0;i<game.letters.Length && i<originalLetters.Length;i++)
                if(game.letters[i]!=null)game.letters[i].transform.position=originalLetters[i];
            for(int i=0;i<hazards.Length;i++)
            {
                var h=hazards[i];if(h==null)continue;var v=originalHazards[i];
                h.safeDuration=v.x;h.warningDuration=v.y;h.dangerDuration=v.z;h.initialDelay=v.w;
            }
        }
        public void Apply()
        {
            if(!randomizeRounds || game==null || game.IsTraining)return;
            LastSeed=fixedSeed>0?fixedSeed:UnityEngine.Random.Range(1,int.MaxValue);
            if(fixedSeed<=0 && LastSeed==previousSeed)LastSeed=LastSeed==int.MaxValue-1?1:LastSeed+1;
            previousSeed=LastSeed;
            var random=new System.Random(LastSeed);
            // Spread the available letters across three regions (12 letters: 4 per region).
            // Keep the original layout if the edited scene does not have enough safe candidates.
            var positions=new List<Vector2>();
            int count=0;foreach(var l in game.letters)if(l!=null)count++;
            int near=Mathf.CeilToInt(count/3f), middle=Mathf.CeilToInt((count-near)/2f);
            bool placed=Choose(lowerCandidates,near,positions,random) &&
                Choose(middleCandidates,middle,positions,random) &&
                Choose(upperCandidates,count-near-middle,positions,random);
            if(placed)
            {
                Shuffle(positions,random);
                int index=0;
                foreach(var letter in game.letters) if(letter!=null)
                {
                    Vector2 pos=positions[index++];
                    letter.transform.position=new Vector3(pos.x,pos.y,letter.transform.position.z);
                }
            }
            else Debug.LogWarning("안전한 편지 후보 위치가 부족해 원래 배치를 사용합니다.",this);
            foreach(var car in game.obstacles)
            {
                if(car==null)continue;
                float min=Mathf.Max(1,minimumCarMultiplier),max=Mathf.Max(min,maximumCarMultiplier);
                car.ResetObstacle(Range(random,min,max),Range(random,.05f,.95f),random.Next(2)==0);
            }
            foreach(var h in hazards)
            {
                if(h==null)continue;
                h.safeDuration=Range(random,1.5f,2.3f);
                h.warningDuration=Range(random,1.0f,1.2f);
                h.dangerDuration=Range(random,1.3f,1.8f);
                h.initialDelay=Range(random,0,1.6f);
                h.ResetHazard(1);
            }
        }
        private bool Choose(Vector2[] candidates,int count,List<Vector2> chosen,System.Random random)
        {
            if(count==0)return true;
            if(candidates==null)return false;
            var pool=new List<Vector2>(candidates);Shuffle(pool,random);
            int added=0;
            foreach(var point in pool)
            {
                if(!Safe(point))continue;
                bool separated=true;
                foreach(var used in chosen)if(Vector2.Distance(point,used)<minimumLetterSpacing){separated=false;break;}
                if(!separated)continue;
                chosen.Add(point);added++;
                if(added==count)return true;
            }
            return false;
        }
        private bool Safe(Vector2 point)
        {
            // Bounds and mailbox are protected; road corridors are excluded even if a car is elsewhere.
            if(Mathf.Abs(point.x)>3.2f || Mathf.Abs(point.y)>4.7f)return false;
            if(Vector2.Distance(point,game.player.spawnPoint)<1.15f)return false;
            if(mailbox!=null && Vector2.Distance(point,mailbox.transform.position)<1.25f)return false;
            foreach(var car in game.obstacles)
                if(car!=null && DistanceToSegment(point,car.startPoint,car.endPoint)<.95f)return false;
            foreach(var h in hazards)
            {
                if(h==null)continue;
                var collider=h.GetComponent<BoxCollider2D>();
                if(collider!=null)
                {
                    Vector2 half=Vector2.Scale(collider.size,new Vector2(Mathf.Abs(h.transform.lossyScale.x),Mathf.Abs(h.transform.lossyScale.y)))*.5f+Vector2.one*.35f;
                    Vector2 center=h.transform.TransformPoint(collider.offset);
                    if(Mathf.Abs(point.x-center.x)<half.x && Mathf.Abs(point.y-center.y)<half.y)return false;
                }
            }
            foreach (var obstacle in Physics2D.OverlapCircleAll(point, .35f))
                if (!obstacle.isTrigger) return false;
            return true;
        }
        private static float DistanceToSegment(Vector2 p,Vector2 a,Vector2 b)
        {
            Vector2 line=b-a;
            float t=line.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,line)/line.sqrMagnitude):0;
            return Vector2.Distance(p,a+line*t);
        }
        private static float Range(System.Random r,float min,float max) => Mathf.Lerp(min,max,(float)r.NextDouble());
        private static void Shuffle<T>(List<T> list,System.Random random)
        {
            for(int i=list.Count-1;i>0;i--){int j=random.Next(i+1);T temp=list[i];list[i]=list[j];list[j]=temp;}
        }
    }
}
