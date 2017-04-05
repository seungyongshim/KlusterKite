import React from 'react'
import Relay from 'react-relay'

import { hasPrivilege } from '../../utils/privileges';
import TemplatesList from '../../components/TemplatesListOld/index';

class TemplatesListPage extends React.Component {
  static propTypes = {
    api: React.PropTypes.object,
  };

  render () {
    return (
      <div>
        <TemplatesList
          templates={this.props.api.clusterKitNodesApi}
          createNodeTemplatePrivilege={hasPrivilege('ClusterKit.NodeManager.NodeTemplate.Create')}
          getNodeTemplatePrivilege={hasPrivilege('ClusterKit.NodeManager.NodeTemplate.Get')} />
      </div>
    )
  }
}

export default Relay.createContainer(
  TemplatesListPage,
  {
    fragments: {
      api: () => Relay.QL`fragment on IClusterKitNodeApi {
        __typename
        clusterKitNodesApi {
          ${TemplatesList.getFragment('templates')},
        }
      }
      `,
    }
  },
)
