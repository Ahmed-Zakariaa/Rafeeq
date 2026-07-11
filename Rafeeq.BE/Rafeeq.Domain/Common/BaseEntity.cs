using System.ComponentModel.DataAnnotations;

namespace Rafeeq.Domain.Common;

/// <summary>Marker for entities that are soft-deleted (never physically removed).</summary>
public interface ICanBeSoftDeleted
{
    bool IsDeleted { get; }
    void Delete();
    void UndoDelete();
}

/// <summary>Audit fields set automatically in <c>SaveChanges</c>.</summary>
public interface IAuditableEntity
{
    int? CreatedBy { get; set; }
    DateTime CreatedDate { get; set; }
    int? ModifiedBy { get; set; }
    DateTime? ModifiedDate { get; set; }
}

/// <summary>
/// Base for all entities: identity, audit, activation, soft-delete and optimistic concurrency.
/// Mutate state through methods on the derived entity — not by setting properties.
/// </summary>
public abstract class BaseEntity<TId> : IAuditableEntity, ICanBeSoftDeleted
{
    public TId Id { get; protected set; } = default!;

    public int? CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    public bool IsActive { get; protected set; } = true;
    public bool IsDeleted { get; protected set; }

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public virtual void Delete() => IsDeleted = true;
    public virtual void UndoDelete() => IsDeleted = false;
    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
}
