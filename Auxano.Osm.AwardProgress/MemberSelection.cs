using Auxano.Osm.Api;

namespace Auxano.Osm.AwardProgress
{
    public class MemberSelection
    {
        public MemberSelection(Member member)
        {
            this.Member = member;
            this.Name = member.FamilyName + ", " + member.FirstName;
        }

        public Member Member { get; private set; }
        public string Name { get; private set; }
        public bool Selected { get; set; }
    }
}