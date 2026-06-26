using UnityEngine;

public enum ProcessType
{
    Cook,
    Grind,
}

public class ProcessingStation : MonoBehaviour
{
    [SerializeField] ProcessType processType;
    [SerializeField] float processTime = 2f;
    [SerializeField] Transform inputDisplay;
    [SerializeField] Transform outputSpawn;
    [SerializeField] float speedMultiplier = 1f;

    private IngredientData currentInput;
    private GameObject currentOutputPrefab;

    private float timer;
    private bool processing;

    private GameObject displayedInput;

    private void OnTriggerEnter(Collider other)
    {
        if (processing)
            return;

        Ingredient ingredient = other.GetComponent<Ingredient>();

        if (ingredient == null)
            return;

        GameObject resultPrefab = GetProcessedResult(processType, ingredient.ingredient);

        if (resultPrefab == null)
            return;

        StartProcessing(ingredient.ingredient, resultPrefab);
        Destroy(other.gameObject);
    }

    private GameObject GetProcessedResult(ProcessType processType, IngredientData ingredient)
    {
        switch (processType)
        {
            case ProcessType.Cook:
                return ingredient.cookedPrefab;

            case ProcessType.Grind:
                return ingredient.crushedPrefab;
        }

        return null;
    }

    private void StartProcessing(IngredientData ingredient, GameObject resultPrefab)
    {
        currentInput = ingredient;
        currentOutputPrefab = resultPrefab;

        timer = 0f;
        processing = true;

        displayedInput = Instantiate(
            ingredient.prefab,
            inputDisplay.position,
            inputDisplay.rotation,
            inputDisplay
        );

        Rigidbody rb = displayedInput.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        if (!processing)
            return;

        timer += Time.deltaTime * speedMultiplier;

        if (timer >= processTime)
        {
            FinishProcessing();
        }
    }

    private void FinishProcessing()
    {
        processing = false;

        if (displayedInput != null)
            Destroy(displayedInput);

        GameObject output = Instantiate(
            currentOutputPrefab,
            outputSpawn.position,
            outputSpawn.rotation
        );

        Rigidbody rb = output.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        currentInput = null;
        currentOutputPrefab = null;
    }

    public void UpgradeSpeed(float newMultiplier)
    {
        speedMultiplier = newMultiplier;
    }
}