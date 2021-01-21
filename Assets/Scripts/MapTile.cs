using Unity.Entities;

namespace FlowFields.Components
{
    public struct MapTile:IComponentData
    {
        public const int ByteSize = 2;
        public byte availableDirectionsCode;
        public byte isOccupied;
    }
}