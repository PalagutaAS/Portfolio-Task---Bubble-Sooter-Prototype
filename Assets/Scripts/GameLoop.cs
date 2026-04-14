using UnityEngine;

public class GameLoop : MonoBehaviour
{
    private GridGenerator _gridGenerator;

    private void Awake()
    {
        Debug.Log("Awake"); 
    }
    
    public void Constructor(IBubbleGridStorage bubbleStorage, IBubbleNeighborFinder neighborFinder,
        IBubbleMatchFinder matchFinder, GridGenerator gridGenerator)
    {
        _gridGenerator = gridGenerator;
    }

    private void Start()
    {
        Debug.Log("Start");
        _gridGenerator.GenerateRandomBubbles();
        //Сгенерировать баблы для выстрелов
        //Ждать выстрела
        //После выстрела проверка на совпадение
        //ПРоверка на соеденение с потолком и удаление
        //Проверка на проигрышь
        //Если проиграли показать UI
        //если нажали рестарт то запустить все сначала
    }
    
}