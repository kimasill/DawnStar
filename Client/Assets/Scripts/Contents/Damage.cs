using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Damage : MonoBehaviour
{
    public TMP_Text damageText;

    public void SetDamage(int damage, bool isCritical)
    {
        damageText.text = damage.ToString();
        damageText.color = isCritical ? Color.red : Color.white;
        damageText.fontSize = isCritical ? 4 : 3;
    }

    public void ShowDamage(Vector3 position)
    {
        // 오브젝트 위쪽 범위에 랜덤 위치 설정
        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.2f), Random.Range(0.5f, 1.0f), 0);
        transform.position = position + randomOffset;
        gameObject.SetActive(true);
        StartCoroutine(RemoveAfterDelay(1.0f));
    }

    private IEnumerator RemoveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}
