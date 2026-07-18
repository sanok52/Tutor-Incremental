using System.Collections;
using System.Linq;
using UnityEngine;

public class People : MonoBehaviour
{
    private void Start()
    {
        TestButtons.SubOnClick("AddRandomReactoions", (info) => AddRandomReactions());

        SocialUIManager.Instance.OnPublicPostChange += PostChangeWork;
        SocialUIManager.Instance.OnPublicPostAdd += PostAddWork;
    }

    public void AddRandomReactions()
    {
        var pub = SocialUIManager.Instance.Publics.ToArray().RandomElement();

        foreach (var post in pub.posts)
        {
            post.AddRandomReactions(Random.Range(0, 100) < 10, Random.Range(0, 5));
        }
    }

    private void PostChangeWork(SocialPublic socialPublic, SocialPost socialPost)
    {

    }

    private void PostAddWork(SocialPublic socialPublic, SocialPost socialPost)
    {
        StartCoroutine(ReactionAtPostRoutine(socialPublic, socialPost));
    }

    private IEnumerator ReactionAtPostRoutine(SocialPublic socialPublic, SocialPost socialPost)
    {
        int borderNegative = 10;
        bool isRepeatPost = socialPublic.posts.Where(x => socialPost.MainImageID == x.MainImageID).Count() >= 2;
        if (isRepeatPost)
        {
            //Повторяется пост, значит негативные реакции будут чаще
            borderNegative = 100;
            Debug.Log("fuuu! Повторяется пост, значит негативные реакции будут чаще");
        }

        int countReactions = (int)(Random.Range(socialPost.virality, 1f) * socialPublic.Overviews) + socialPublic.Subscrubers;
        for (int i = 0; i < countReactions; i++)
        {
            yield return new WaitForSeconds(Random.Range(0f, 5f));

            int count = Mathf.Clamp(Random.Range(0, 3), 1, countReactions);
            socialPost.AddRandomReactions(Random.Range(0, 100) < borderNegative, count);

            if (isRepeatPost)
                InterfaceManager.CreateFlyingText("Было!", Color.red, Random.insideUnitCircle,
                    FindFirstObjectByType<MessangerWindow>().transform, true);
            else
                InterfaceManager.CreateFlyingText(GetGoodComment(), Color.yellow, Random.insideUnitCircle,
                    FindFirstObjectByType<MessangerWindow>().transform, true);

            countReactions -= count;
        }
    }

    private string GetGoodComment()
    {
        string[] comments = new string[]
        {
            "Классно!",
            "Супер!",
            "Вау!",
            "Красотка!",
            "Милота!",
            "Жена!"
        };
        return comments.RandomElement();
    }
}