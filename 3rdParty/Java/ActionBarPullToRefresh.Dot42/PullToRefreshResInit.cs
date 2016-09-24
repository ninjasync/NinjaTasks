
// Copy this file to you project, copy or link the the resources,
// update the namespaces, and InitResources() at the start of you app.

namespace NinjaTasks.App.Droid.Resources3rdParty
{
    using RS = R;
    using RT = Uk.Co.Senab.Actionbarpulltorefresh.Library.R;

    public static class PullToRefreshResInit
    {
        public static void InitResources()
        {
            RT.Id.Ptr_progress = RS.Id.ptr_progress;
            RT.Id.Ptr_content = RS.Id.ptr_content;
            RT.Id.Ptr_text = RS.Id.ptr_text;

            RT.String.Pull_to_refresh_pull_label = RS.String.pull_to_refresh_pull_label;
            RT.String.Pull_to_refresh_refreshing_label = RS.String.pull_to_refresh_refreshing_label;
            RT.String.Pull_to_refresh_release_label = RS.String.pull_to_refresh_release_label;

            RT.Attr.PtrHeaderStyle = RS.Attr.ptrHeaderStyle;

            RT.Layout.Default_header = RS.Layout.default_header;
            
            RT.Styleable.PullToRefreshHeader_ptrHeaderHeight = RS.Styleable.PullToRefreshHeader.ptrHeaderHeight;
            RT.Styleable.PullToRefreshHeader_ptrHeaderTitleTextAppearance = RS.Styleable.PullToRefreshHeader.ptrHeaderTitleTextAppearance;
            RT.Styleable.PullToRefreshHeader_ptrProgressBarColor = RS.Styleable.PullToRefreshHeader.ptrProgressBarColor;
            RT.Styleable.PullToRefreshHeader_ptrPullText = RS.Styleable.PullToRefreshHeader.ptrPullText;
            RT.Styleable.PullToRefreshHeader_ptrRefreshingText = RS.Styleable.PullToRefreshHeader.ptrRefreshingText;
            RT.Styleable.PullToRefreshHeader_ptrReleaseText = RS.Styleable.PullToRefreshHeader.ptrReleaseText;
            RT.Styleable.PullToRefreshHeader_ptrHeaderBackground = RS.Styleable.PullToRefreshHeader.ptrHeaderBackground;
            RT.Styleable.PullToRefreshHeader = RS.Styleable.PullToRefreshHeader.AllIds;
        }
    }
}
