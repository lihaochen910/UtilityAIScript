using System.Collections.Generic;


namespace Pixpil.AI.UtilityAIScript.Ast;

/// <summary>
/// Abstract node.
/// </summary>
public abstract class Node {
	
	/// <summary>
    /// list of childrens for ast navigation.
    /// </summary>
    private List<Node> _childrenList;
    private Dictionary<string, object> _tags;

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    protected Node()
    {
    }

    /// <summary>
    /// Gets or sets the source span.
    /// </summary>
    /// <value>
    /// The source span.
    /// </value>
    public SourceSpan Span { get; set; }


    public override bool Equals(object against)
    {
        return base.Equals(against);
    }
    
    public static bool operator ==(Node left, Node right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Node left, Node right)
    {
        return !Equals(left, right);
    }

    /// <summary>
    /// Gets the childrens.
    /// </summary>
    public List<Node> ChildrenList => _childrenList ??= new List< Node >();


    /// <summary>
    /// Gets or sets tags collection.
    /// </summary>
    public Dictionary<string, object> Tags
    {
        get => _tags;
        set => _tags = value;
    }

    /// <summary>
    /// Gets a tag value associated to this node..
    /// </summary>
    /// <param name="tagKey">The tag key.</param>
    /// <returns>The tag value</returns>
    public object GetTag(string tagKey)
    {
        if (_tags == null) return null;
        object result;
        _tags.TryGetValue(tagKey, out result);
        return result;
    }

    /// <summary>
    /// Gets a tag value associated to this node..
    /// </summary>
    /// <param name="tagKey">The tag key.</param>
    /// <returns>The tag value</returns>
    public bool RemoveTag(string tagKey)
    {
        if (_tags == null) return true;
        return _tags.Remove(tagKey);
    }

    /// <summary>
    /// Determines whether the specified instance contains this tag.
    /// </summary>
    /// <param name="tagKey">The tag key.</param>
    /// <returns>
    ///   <c>true</c> if the specified instance contains this tag; otherwise, <c>false</c>.
    /// </returns>
    public bool ContainsTag(string tagKey)
    {
        if (_tags == null) return false;
        return _tags.ContainsKey(tagKey);
    }

    /// <summary>
    /// Sets a tag value associated to this node.
    /// </summary>
    /// <param name="tagKey">The tag key.</param>
    /// <param name="tagValue">The tag value.</param>
    public void SetTag(string tagKey, object tagValue)
    {
        _tags ??= new Dictionary< string, object >();
        _tags.Remove(tagKey);
        _tags.Add(tagKey, tagValue);
    }

    public void AddChild< T >( T child ) where T : Node {
        ChildrenList.Add( child );
    }

    /// <summary>
    /// Gets the child nodes.
    /// </summary>
    /// <returns>An enumeration of child nodes</returns>
    public virtual IEnumerable<Node> Childrens()
    {
        return ChildrenList;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetType().Name;
    }
    
}
