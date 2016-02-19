using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Auxano.Osm.Api
{
    /// <summary>
    /// Holds an array of terms.
    /// </summary>
    public class TermArray
        : IEnumerable<Term>
    {
        private readonly Term current;
        private readonly ImmutableArray<Term> terms;

        /// <summary>
        /// Initialises a new instance of <see cref="TermArray"/>.
        /// </summary>
        /// <param name="terms">The terms in the array.</param>
        /// <param name="current">The curren term.</param>
        public TermArray(IEnumerable<Term> terms, Term current = null)
        {
            this.terms = ImmutableArray.Create(terms.ToArray());
            var today = DateTime.UtcNow.Date;
            this.current = current ??
                (this.terms.Any() ? this.terms.FirstOrDefault(t => (t.StartDate <= today) && (t.EndDate >= today)) : null);
        }

        /// <summary>
        /// The currently active term.
        /// </summary>
        public Term Current
        {
            get { return this.current; }
        }

        /// <summary>
        /// Changes the currently active active.
        /// </summary>
        /// <param name="term">The new active term.</param>
        /// <returns>A new <see cref="TermArray"/> instance.</returns>
        public TermArray ChangeCurrentTerm(Term term)
        {
            return new TermArray(this.terms, term);
        }

        /// <summary>
        /// Gets the enumerator for iterating through the list of terms.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Term> GetEnumerator()
        {
            return this.terms
                .AsEnumerable()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return terms
                .AsQueryable()
                .GetEnumerator();
        }
    }
}