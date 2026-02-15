// <copyright file="KeyApi.KeyInfo.cs" company="improvGroup, LLC">
//     Copyright © 2015-2022 Richard Schneider, Marshall Rosenstein, William Forney
// </copyright>
namespace Ipfs.Http.Client.CoreApi;

/// <summary>
/// Class KeyApi.
/// Implements the <see cref="Ipfs.CoreApi.IKeyApi" />
/// </summary>
/// <seealso cref="Ipfs.CoreApi.IKeyApi" />
public partial class KeyApi
{
    /// <summary>
    /// Information about a local key.
    /// </summary>    
    public class KeyInfo : IKey
    {
        /// <inheritdoc />
        public Cid Id { get; set; } = new Cid();

        /// <inheritdoc />
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc />
        public override string ToString() => this.Name;
    }
}
