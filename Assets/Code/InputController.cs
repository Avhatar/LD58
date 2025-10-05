using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private LayerMask tileLayerMask;
    [SerializeField] private LayerMask itemLayerMask;
    [SerializeField] private LayerMask fireplaceLayerMask;
    [SerializeField] private LayerMask buyerLayerMask;

    private Camera mainCamera;
    private Tile hoveredTile;
    private Item hoveredItem;
    private Fireplace hoveredFireplace;
    private Buyer hoveredBuyer;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (UI.InputLocked)
            return;
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        UpdateHover();

        if (mouse.leftButton.wasPressedThisFrame)
            HandleLeftClick();
    }

    private void UpdateHover()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        Item hitItem = null;
        Tile hitTile = null;
        Fireplace hitFireplace = null;
        Buyer hitBuyer = null;

        if (Physics.Raycast(ray, out RaycastHit hitItemCast, 1000f, itemLayerMask))
            hitItem = hitItemCast.collider.GetComponentInParent<Item>();
        else if (Physics.Raycast(ray, out RaycastHit hitBuyerCast, 1000f, buyerLayerMask))
            hitBuyer = hitBuyerCast.collider.GetComponentInParent<Buyer>();
        else if (Physics.Raycast(ray, out RaycastHit hitFireplaceCast, 1000f, fireplaceLayerMask))
            hitFireplace = hitFireplaceCast.collider.GetComponentInParent<Fireplace>();
        else if (Physics.Raycast(ray, out RaycastHit hitTileCast, 1000f, tileLayerMask))
            hitTile = hitTileCast.collider.GetComponentInParent<Tile>();

        if (hitItem != hoveredItem)
        {
            if (hoveredItem != null)
                hoveredItem.highlighter.ConstantOff();
            hoveredItem = hitItem;
            if (hoveredItem != null)
                hoveredItem.highlighter.ConstantOnCustomColor(Core.instance.globalSettings.targetItemColor);
        }

        if (hitBuyer != hoveredBuyer)
        {
            if (hoveredBuyer != null)
                hoveredBuyer.highlighter.ConstantOff();
            hoveredBuyer = hitBuyer;
            if (hoveredBuyer != null)
                hoveredBuyer.highlighter.ConstantOnCustomColor(Core.instance.globalSettings.targetSelectColor);
        }

        if (hitFireplace != hoveredFireplace)
        {
            if (hoveredFireplace != null)
                hoveredFireplace.highlighter.ConstantOff();
            hoveredFireplace = hitFireplace;
            if (hoveredFireplace != null)
                hoveredFireplace.highlighter.ConstantOnCustomColor(Core.instance.globalSettings.targetSelectColor);
        }

        if (hitTile != hoveredTile)
        {
            if (hoveredTile != null)
                hoveredTile.highlighter.ConstantOff();
            hoveredTile = hitTile;
            if (hoveredTile != null && hoveredItem == null && hoveredFireplace == null && hoveredBuyer == null)
                hoveredTile.highlighter.ConstantOnCustomColor(Core.instance.globalSettings.targetSelectColor);
        }

        if (hoveredBuyer != null && hoveredTile != null)
        {
            hoveredTile.highlighter.ConstantOff();
            hoveredTile = null;
        }
        if (hoveredItem != null && hoveredTile != null)
        {
            hoveredTile.highlighter.ConstantOff();
            hoveredTile = null;
        }
        if (hoveredFireplace != null && hoveredTile != null)
        {
            hoveredTile.highlighter.ConstantOff();
            hoveredTile = null;
        }
    }

    private void HandleLeftClick()
    {
        var mainCharacter = Core.instance.MainCharacter;
        if (mainCharacter == null) return;

        if (TryGetItem(out Item item))
        {
            mainCharacter.GoToPickup(item);
            return;
        }

        if (TryGetBuyer(out Buyer buyer))
        {
            mainCharacter.GoSell(buyer);
            return;
        }

        if (TryGetFireplace(out Fireplace fireplace))
        {
            mainCharacter.GoAddCoal(fireplace);
            return;
        }

        if (TryGetTilePoint(out Tile tile, out Vector3 tilePoint))
        {
            mainCharacter.GoDig(tile);
            return;
        }

        if (TryGetGroundPoint(out Vector3 groundPoint))
        {
            mainCharacter.GoToPoint(groundPoint);
        }
    }

    private bool TryGetGroundPoint(out Vector3 point)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayerMask))
        {
            point = hit.point;
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    private bool TryGetTilePoint(out Tile tile, out Vector3 point)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, tileLayerMask))
        {
            tile = hit.collider.GetComponentInParent<Tile>();
            point = hit.collider.bounds.center;
            return true;
        }

        tile = null;
        point = Vector3.zero;
        return false;
    }

    private bool TryGetItem(out Item item)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, itemLayerMask))
        {
            item = hit.collider.GetComponentInParent<Item>();
            return true;
        }

        item = null;
        return false;
    }

    private bool TryGetFireplace(out Fireplace fireplace)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, fireplaceLayerMask))
        {
            fireplace = hit.collider.GetComponentInParent<Fireplace>();
            return true;
        }

        fireplace = null;
        return false;
    }

    private bool TryGetBuyer(out Buyer buyer)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, buyerLayerMask))
        {
            buyer = hit.collider.GetComponentInParent<Buyer>();
            return true;
        }

        buyer = null;
        return false;
    }
}