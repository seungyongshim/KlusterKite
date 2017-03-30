import React from 'react'
import Relay from 'react-relay'

import FeedsList from '../../components/FeedsList/index';

class FeedsListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    return (
      <div>
        <FeedsList feeds={this.props.api.clusterKitNodesApi} />
      </div>
    )
  }
}

// ${FeedsList.getFragment('feeds')},
export default Relay.createContainer(
  FeedsListPage,
  {
    fragments: {
      api: () => Relay.QL`fragment on IClusterKitNodeApi {
        __typename
        clusterKitNodesApi {
          ${FeedsList.getFragment('feeds')},
        }
      }
      `,
    }
  },
)
