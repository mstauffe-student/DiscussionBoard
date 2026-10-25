namespace DiscussionBoard.Models
{
    /**
     * A base class for all db entities to inherit from.
     */
    public abstract class EntityBase
    {
        public int Id { get; set; }
        public Boolean IsDeleted { get; set; }
    }
}
