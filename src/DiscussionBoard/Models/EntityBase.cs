namespace DiscussionBoard.Models
{
    /**
     * A base class for all db entites to inherient from.
     */
    public abstract class EntityBase
    {
        public int Id { get; set; }
        public Boolean IsDeleted { get; set; }
    }
}
